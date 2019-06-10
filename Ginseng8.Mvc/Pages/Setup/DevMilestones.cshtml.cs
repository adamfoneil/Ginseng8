using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public int? MilestoneId { get; set; }

        public SelectList AppSelect { get; set; }
        public SelectList MilestoneSelect { get; set; }
        public SelectList UserSelect { get; set; }

        public IEnumerable<DeveloperMilestone> Developers { get; set; }

        public Dictionary<int, int> AwayHours { get; set; }

        public Dictionary<int, DevMilestoneWorkingHoursResult> DevLoad { get; set; }

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                AppSelect = await new AppSelect() { OrgId = OrgId }.ExecuteSelectListAsync(cn, AppId);
                MilestoneSelect = await new MilestoneSelect() { AppId = AppId ?? 0 }.ExecuteSelectListAsync(cn, MilestoneId);
                UserSelect = await new UserSelect() { OrgId = OrgId, IsEnabled = true }.ExecuteSelectListAsync(cn);

                if (MilestoneId.HasValue)
                {
                    var ms = await cn.FindAsync<Milestone>(MilestoneId.Value);
                    Developers = await new DevMilestones() { OrgId = OrgId, MilestoneId = MilestoneId.Value }.ExecuteAsync(cn);

                    if (Developers.Any())
                    {
                        var awayDays = await new UserAwayDays()
                        {
                            OrgId = OrgId,
                            StartDate = Developers.Min(row => row.StartDate),
                            EndDate = ms.Date
                        }.ExecuteAsync(cn);
                        AwayHours = awayDays.ToDictionary(row => row.UserId, row => row.TotalHours);

                        var load = await new DevMilestoneWorkingHours() { OrgId = OrgId, MilestoneId = MilestoneId }.ExecuteAsync(cn);
                        DevLoad = load.ToDictionary(row => row.DeveloperId);
                    }
                }
            }
        }

        public int GetAwayHours(int userId)
        {
            return (AwayHours.ContainsKey(userId)) ? AwayHours[userId] : 0;
        }

        public int GetLoadHours(int userId, Func<DevMilestoneWorkingHoursResult, int> getter)
        {
            return (DevLoad.ContainsKey(userId)) ? getter.Invoke(DevLoad[userId]) : 0;
        }

        public async Task<RedirectResult> OnPostSaveAsync(DeveloperMilestone record)
        {
            var ms = await Data.FindAsync<Milestone>(record.MilestoneId);
            await Data.TrySaveAsync(record);
            return Redirect($"/Setup/DevMilestones?appId={ms.ApplicationId}&milestoneId={record.MilestoneId}");
        }

        public async Task<RedirectResult> OnPostDeleteAsync(int id)
        {
            var devMs = await Data.FindAsync<DeveloperMilestone>(id);
            await Data.TryDeleteAsync<DeveloperMilestone>(id);
            return Redirect($"/Setup/DevMilestones?appId={devMs.Milestone.ApplicationId}&milestoneId={devMs.MilestoneId}");
        }
    }
}