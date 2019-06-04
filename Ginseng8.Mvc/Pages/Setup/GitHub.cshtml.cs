using Ginseng.Mvc.Queries.SelectLists;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Ginseng.Mvc.Interfaces;

namespace Ginseng.Mvc.Pages.Setup
{
    public class GitHubModel : AppPageModel
    {
        private readonly IGitHubService _gitHub;

        public GitHubModel(
            IConfiguration config,
            IGitHubService gitHub) 
            : base(config)
        {
            _gitHub = gitHub;
        }

        public SelectList AppSelect { get; set; }
        public SelectList RepoSelect { get; set; }

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                AppSelect = await new AppSelect() { OrgId = OrgId }.ExecuteSelectListAsync(cn);
            }
        }
    }
}