using Ginseng.Mvc.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ginseng.Mvc.Controllers
{
    public class GitHubController : Controller
    {
        private readonly IGitHubService _gitHub;

        public GitHubController(
            IGitHubService gitHub)
        {
            _gitHub = gitHub;
        }
    }
}