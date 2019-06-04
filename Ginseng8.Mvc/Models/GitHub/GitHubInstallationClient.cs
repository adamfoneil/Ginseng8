using System;
using Octokit;

namespace Ginseng.Mvc.Models.GitHub
{
    /// <summary>
    /// GitHub App installation client
    /// </summary>
    public class GitHubInstallationClient
    {
        /// <summary>
        /// GitHub App installation client
        /// </summary>
        public GitHubClient Client { get; set; }

        /// <summary>
        /// GitHub App installation client's access token expiration date
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
