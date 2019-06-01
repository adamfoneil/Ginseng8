using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace Ginseng.Mvc.Interfaces
{
    public interface IGitHubService
    {
        GitHubClient GetClient();

        string GetToken(string code);

        /// <summary>
        /// Should include public and private
        /// </summary>
        Task<IEnumerable<Repository>> ListRepositoriesAsync();

        /// <summary>
        /// List branches of the given repo Id
        /// </summary>
        Task<IEnumerable<Branch>> ListBranchesAsync(int repositoryId);

        /// <summary>
        /// Associates commit messages with work items based on the branch and message formatting
        /// </summary>
        Task OnWebhookPushAsync(PushEventPayload @event);
    }
}