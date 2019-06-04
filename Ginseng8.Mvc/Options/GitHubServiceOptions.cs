using Ginseng.Mvc.Interfaces;

namespace Ginseng.Mvc.Options
{
    /// <summary>
    /// GitHub service configuration options
    /// </summary>
    public class GitHubServiceOptions : IConfigurationOptions
    {
        /// <summary>
        /// GitHub App id
        /// </summary>
        /// <remarks>
        /// https://developer.github.com/apps/building-github-apps/creating-a-github-app/
        /// </remarks>
        public int AppId { get; set; }

        /// <summary>
        /// GitHub App name
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// GitHub App private key
        /// </summary>
        /// <remarks>
        /// https://developer.github.com/apps/building-github-apps/authenticating-with-github-apps/#generating-a-private-key
        /// </remarks>
        public string AppPrivateKey { get; set; }

        
    }
}
