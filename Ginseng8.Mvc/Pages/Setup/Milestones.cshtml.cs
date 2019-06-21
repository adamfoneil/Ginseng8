using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
    public class MilestonesModel : AppPageModel
    {
        public MilestonesModel(IConfiguration config) : base(config)
        {
        }

        [BindProperty(SupportsGet = true)]
        public int AppId { get; set; }

        public SelectList AppSelect { get; set; }
        public IEnumerable<Milestone> Milestones { get; set; }

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                AppSelect = await new AppSelect() { OrgId = OrgId }.ExecuteSelectListAsync(cn, AppId);
                Milestones = await new Milestones() { OrgId = OrgId, AppId = AppId }.ExecuteAsync(cn);
            }
        }

        public async Task<ActionResult> OnPostSave(Milestone record)
        {
            await Data.TrySaveAsync(record);
            return Redirect($"/Setup/Milestones?appId={record.ApplicationId}");
        }

        public async Task<ActionResult> OnPostDelete(int id)
        {
            var ms = await Data.FindAsync<Milestone>(id);
            await Data.TryDeleteAsync<Milestone>(id);
            return Redirect($"/Setup/Milestones?appId={ms.ApplicationId}");
        }
    }
}