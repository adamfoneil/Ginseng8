using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Controllers
{
    public class GitHubController : Controller
    {
        private readonly GitHubService _gitHub;

        public GitHubController(IConfiguration config)
        {
            _gitHub = new GitHubService(config);
        }

        public RedirectResult Callback(string code)
        {
            string token = _gitHub.GetToken(code);
            var bytes = Encoding.UTF8.GetBytes(token);
            HttpContext.Session.Set("AccessToken", bytes);

            throw new NotImplementedException();

        }
    }
}