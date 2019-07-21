using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Setup
{
    public class GitHubModel : AppPageModel
    {
        public GitHubModel(IConfiguration config) : base(config)
        {
        }

        public void OnGet()
        {
        }
    }
}