using Ginseng.Mvc.Interfaces;
using Microsoft.Extensions.Configuration;
using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly DataAccess _data;

        public GitHubService(IConfiguration config)
        {
            _clientId = config.GetSection("GitHub").GetValue<string>("ClientId");
            _clientSecret = config.GetSection("GitHub").GetValue<string>("ClientSecret");
            _data = new DataAccess(config);
        }

        public GitHubClient GetClient()
        {
            return new GitHubClient(new ProductHeaderValue("Ginseng"));
        }

        public string GetToken(string code)
        {
            var request = new OauthLoginRequest(_clientId);

            // https://github.com/octokit/octokit.net/blob/master/docs/oauth-flow.md

            throw new NotImplementedException();
        }

        public Task<IEnumerable<Branch>> ListBranchesAsync(int repositoryId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Repository>> ListRepositoriesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task OnWebhookPushAsync(PushEventPayload @event)
        {
            throw new NotImplementedException();
        }
    }
}