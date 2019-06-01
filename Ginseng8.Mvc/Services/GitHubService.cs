using System;
using Microsoft.Extensions.Configuration;
using Octokit;

namespace Ginseng.Mvc.Services
{
    public class GitHubService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;

        public GitHubService(IConfiguration config)
        {
            _clientId = config.GetSection("GitHub").GetValue<string>("ClientId");
            _clientSecret = config.GetSection("GitHub").GetValue<string>("ClientSecret");
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
    }
}