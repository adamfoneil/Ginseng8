using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
    public class GitHubModel : AppPageModel
    {
        public GitHubModel(IConfiguration config) : base(config)
        {
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