using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Setup
{
    public class DevMilestonesModel : AppPageModel
    {
        public DevMilestonesModel(IConfiguration config) : base(config)
        {
        }

        [BindProperty(SupportsGet = true)]
        public int? AppId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }

        public SelectList AppSelect { get; set; }
        public SelectList MilestoneSelect { get; set; }

        public SelectList UserSelect { get; set; }

        public IEnumerable<DeveloperMilestone> Developers { get; set; }

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                AppSelect = await new AppSelect() { OrgId = OrgId }.ExecuteSelectListAsync(cn, AppId);
                MilestoneSelect = await new MilestoneSelect() { AppId = AppId ?? 0 }.ExecuteSelectListAsync(cn, Id);
                UserSelect = await new UserSelect() { OrgId = OrgId, IsEnabled = true }.ExecuteSelectListAsync(cn);

                if (Id.HasValue)
                {
                    Developers = await new DevMilestones() { OrgId = OrgId }.ExecuteAsync(cn);
                }
            }
        }
    }
}