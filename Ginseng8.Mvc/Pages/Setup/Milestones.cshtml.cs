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
        public int TeamId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? AppId { get; set; }

        public SelectList TeamSelect { get; set; }
        public SelectList AppSelect { get; set; }
        public IEnumerable<Milestone> Milestones { get; set; }

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                TeamSelect = await new TeamSelect() { OrgId = OrgId }.ExecuteSelectListAsync(cn, TeamId);
                AppSelect = await new AppSelect() { OrgId = OrgId, TeamId = TeamId }.ExecuteSelectListAsync(cn, AppId);
                Milestones = await new Milestones() { OrgId = OrgId, TeamId = TeamId, AppId = AppId, IsSelectable = null, MinDate = null }.ExecuteAsync(cn);
            }
        }

        public async Task<ActionResult> OnPostSave(Milestone record)
        {
            await Data.TrySaveAsync(record);
            return Redirect($"/Setup/Milestones?teamId={record.TeamId}&appId={record.ApplicationId}");
        }

        public async Task<ActionResult> OnPostDelete(int id)
        {
            var ms = await Data.FindAsync<Milestone>(id);
            await Data.TryDeleteAsync<Milestone>(id);
            return Redirect($"/Setup/Milestones?teamId={ms.TeamId}&appId={ms.ApplicationId}");
        }
    }
}