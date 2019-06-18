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
    public class ApplicationsModel : AppPageModel
    {
        public ApplicationsModel(IConfiguration config) : base(config)
        {
        }

        [BindProperty(SupportsGet = true)]
        public bool IsActive { get; set; } = true;

        [BindProperty(SupportsGet = true)]
        public int? FilterTeamId { get; set; }

        public SelectList TeamSelect { get; set; }
        public IEnumerable<Application> Applications { get; set; }

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                TeamSelect = await new TeamSelect() { OrgId = OrgId }.ExecuteSelectListAsync(cn, FilterTeamId);
                Applications = await new Applications() { OrgId = OrgId, IsActive = IsActive, TeamId = FilterTeamId }.ExecuteAsync(cn);
            }
        }

        public async Task<ActionResult> OnPostSave(Application app)
        {
            app.OrganizationId = OrgId;
            if (app.TeamId == 0) app.TeamId = null;
            await Data.TrySaveAsync(app);
            return RedirectToPage("/Setup/Applications");
        }

        public async Task<ActionResult> OnPostDelete(int id)
        {
            await Data.TryDeleteAsync<Application>(id);
            return RedirectToPage("/Setup/Applications");
        }
    }
}