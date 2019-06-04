using Ginseng.Mvc.Interfaces;
using Octokit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ginseng.Mvc.Models.GitHub;
using Ginseng.Mvc.Models.GitHub.Dto;
using Ginseng.Mvc.Options;
using GitHubJwt;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Ginseng.Mvc.Services
{
    /// <summary>
    /// GitHub integration service
    /// </summary>
    public class GitHubService : IGitHubService
    {
        /// <summary>
        /// Data access service
        /// </summary>
        private readonly IDataAccess _data;

        /// <summary>
        /// Service options
        /// </summary>
        private readonly GitHubServiceOptions _options;

        /// <summary>
        /// GitHub API JWT token factory (generator)
        /// </summary>
        private readonly IGitHubJwtFactory _tokenFactory;

        /// <summary>
        /// GitHub App installations cache
        /// </summary>
        private readonly ConcurrentDictionary<long, GitHubInstallation> _installations;

        /// <summary>
        /// GitHub repositories cache
        /// </summary>
        private readonly ConcurrentDictionary<long, Repository> _repositories;

        /// <summary>
        /// Determines that Installations cache is populated />
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Service options</param>
        /// <param name="dataAccess">Data access service</param>
        public GitHubService(
            IOptions<GitHubServiceOptions> options,
            IDataAccess dataAccess)
        {
            _data = dataAccess;
            _options = options.Value;

            _installations = new ConcurrentDictionary<long, GitHubInstallation>();
            _repositories = new ConcurrentDictionary<long, Repository>();

            _tokenFactory = new GitHubJwtFactory(
                new StringPrivateKeySource(_options.AppPrivateKey),
                new GitHubJwtFactoryOptions
                {
                    AppIntegrationId = _options.AppId,
                    ExpirationSeconds = 600 // 10 minutes is the maximum time allowed
                }
            );
        }

        /// <inheritdoc />
        public GitHubClient GetClient()
        {
            var jwtToken = _tokenFactory.CreateEncodedJwtToken();
            var clientInfo = new ProductHeaderValue(_options.AppName);
            var client = new GitHubClient(clientInfo)
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            return client;
        }

        /// <inheritdoc />
        public async Task<GitHubInstallationClient> GetInstallationClientAsync(long installationId)
        {
            var client = GetClient();
            var tokenResponse = await client.GitHubApps.CreateInstallationToken(installationId);
            
            var clientInfo = new ProductHeaderValue($"{_options.AppName}-installation_{installationId}");
            var installationClient = new GitHubClient(clientInfo)
            {
                Credentials = new Credentials(tokenResponse.Token)
            };
            
            return new GitHubInstallationClient()
            {
                Client = installationClient,
                ExpiresAt = tokenResponse.ExpiresAt
            };
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Installation>> ListInstallationsAsync()
        {
            var client = GetClient();
            var installations = await client.GitHubApps.GetAllInstallationsForCurrent();

            return installations;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Repository>> ListRepositoriesAsync()
        {
            // populate the cache if it's empty
            await InitAsync(false);

            return _repositories.Values;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Repository>> ListRepositoriesAsync(GitHubClient client)
        {
            // "GET /installation/repositories" endpoint is not implemented in Octokit.NET yet: https://github.com/octokit/octokit.net/issues/1825
            // so, let's do it by ourselves manually 

            var restClient = new RestClient(client.BaseAddress);
            restClient
                .AddDefaultHeader("Accept", "application/vnd.github.machine-man-preview+json")
                .AddDefaultHeader("Authorization", $"token {client.Credentials.Password}");

            var request = new RestRequest("/installation/repositories");
            var response = await restClient.GetAsync<ListRepositoriesResponse>(request);

            return response.Repositories;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Branch>> ListBranchesAsync(long repositoryId)
        {
            // populate the cache if it's empty
            await InitAsync(false);

            var installation = _installations.Values.FirstOrDefault(i => i.Repositories.ContainsKey(repositoryId));
            if (installation == null)
                throw new Exception($"GitHub repository with id {repositoryId} was not found across all GitHub App installations");

            await EnsureInstallationAsync(installation);

            var branches = await ListBranchesAsync(installation.Client, repositoryId);
            return branches;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Branch>> ListBranchesAsync(GitHubClient client, long repositoryId)
        {
            var branches = await client.Repository.Branch.GetAll(repositoryId);
            return branches;
        }

        /// <inheritdoc />
        public Task OnWebhookPushAsync(PushEventPayload @event)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes GitHub service (retrieve and cache all GitHub App's installations and their repositories)
        /// </summary>
        /// <remarks>
        /// This works if installations are a bit, otherwise this approach and entire service should be refactored
        /// in order to access only the interested GitHub App installation
        /// </remarks>
        /// <param name="force">Force refresh the cache</param>
        /// <returns>Task</returns>
        private async Task InitAsync(bool force)
        {
            if (_isInitialized && !force) return;

            _installations.Clear();
            _repositories.Clear();

            var client = GetClient();
            var installations = await client.GitHubApps.GetAllInstallationsForCurrent();

            foreach (var installation in installations)
            {
                var installationClient = await GetInstallationClientAsync(installation.Id);
                var installationInfo = new GitHubInstallation()
                {
                    Client = installationClient.Client,
                    ExpiresAt = installationClient.ExpiresAt,
                    Installation = installation,
                    Repositories = new ConcurrentDictionary<long, Repository>()
                };

                var repositories = await ListRepositoriesAsync(installationInfo.Client);
                foreach (var repository in repositories)
                {
                    installationInfo.Repositories.TryAdd(repository.Id, repository);

                    if (_repositories.ContainsKey(repository.Id)) continue;
                    _repositories.TryAdd(repository.Id, repository);
                }

                _installations.TryAdd(installationInfo.Installation.Id, installationInfo);
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Ensures the installation is valid and updates if not
        /// </summary>
        /// <param name="installation">GitHub App installation to check</param>
        /// <returns>Task</returns>
        private async Task EnsureInstallationAsync(GitHubInstallation installation)
        {
            if (!installation.IsExpired) return;

            var installationClient = await GetInstallationClientAsync(installation.Installation.Id);
            installation.Client = installationClient.Client;
            installation.ExpiresAt = installationClient.ExpiresAt;
        }
    }
}