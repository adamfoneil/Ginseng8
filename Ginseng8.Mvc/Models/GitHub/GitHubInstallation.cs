using System;
using System.Collections.Concurrent;
using Octokit;

namespace Ginseng.Mvc.Models.GitHub
{
    /// <summary>
    /// GitHub App installation
    /// </summary>
    public class GitHubInstallation
    {
        /// <summary>
        /// GitHub App installation client
        /// </summary>
        public GitHubClient Client { get; set; }

        /// <summary>
        /// GitHub App installation client's access token expiration date
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// Determines if current installation is expired or not
        /// </summary>
        public bool IsExpired => ExpiresAt.UtcDateTime <= DateTime.UtcNow;

        /// <summary>
        /// GitHub App installation details
        /// </summary>
        public Installation Installation { get; set; }
        
        /// <summary>
        /// GitHub repositories that the current installation has access to
        /// </summary>
        public ConcurrentDictionary<long, Repository> Repositories { get; set; }
    }
}
