using System.Collections.Generic;
using System.Threading.Tasks;
using Ginseng.Mvc.Models.GitHub;
using Octokit;

namespace Ginseng.Mvc.Interfaces
{
    /// <summary>
    /// GitHub integration interface
    /// </summary>
    public interface IGitHubService
    {

        /// <summary>
        /// Creates GitHub API client behalf of a GitHub App
        /// </summary>
        /// <returns>GitHub API client</returns>
        GitHubClient GetClient();

        /// <summary>
        /// Creates GitHub API installation client behalf of a GitHub App installed on a user
        /// </summary>
        /// <returns>GitHub API installation client</returns>
        Task<GitHubInstallationClient> GetInstallationClientAsync(long installationId);

        /// <summary>
        /// Retrieves a list of all GitHub App installations
        /// </summary>
        /// <returns>List of installations</returns>
        Task<IEnumerable<Installation>> ListInstallationsAsync();

        /// <summary>
        /// Retrieves a list of all repositories that GitHub App has access to across all installations
        /// </summary>
        /// <returns>List of GitHub repositories</returns>
        Task<IEnumerable<Repository>> ListRepositoriesAsync();

        /// <summary>
        /// Retrieves a list of all repositories that GitHub App has access to through the specified installation client
        /// </summary>
        /// <param name="client">GitHub App installation client</param>
        /// <returns>List of GitHub repositories</returns>
        Task<IEnumerable<Repository>> ListRepositoriesAsync(GitHubClient client);

        /// <summary>
        /// Retrieves a list of all repository branches across all installations
        /// </summary>
        /// <param name="repositoryId">GitHub repository id</param>
        /// <returns>List of repository branches</returns>
        Task<IEnumerable<Branch>> ListBranchesAsync(long repositoryId);

        /// <summary>
        /// Retrieves a list of all repository branches through the specified installation client
        /// </summary>
        /// <param name="client">GitHub App installation client</param>
        /// <param name="repositoryId">GitHub repository id</param>
        /// <returns>List of repository branches</returns>
        Task<IEnumerable<Branch>> ListBranchesAsync(GitHubClient client, long repositoryId);

        /// <summary>
        /// Associates commit messages with work items based on the branch and message formatting
        /// Vadim, don't worry about this one -- AO will implement
        /// </summary>
        Task OnWebhookPushAsync(PushEventPayload @event);

        // webhook for pull requests?
    }
}