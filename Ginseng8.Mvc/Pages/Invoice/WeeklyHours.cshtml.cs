using Ginseng.Mvc.Queries;
using Ginseng.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Invoice
{
    public class WeeklyHoursModel : AppPageModel
    {
        public WeeklyHoursModel(IConfiguration config) : base(config)
        {
        }

        public WeeklyHoursView Results { get; set; }

        public SelectList WeekSelect { get; set; }

        public async Task OnGetAsync(int weeksBack = 0)
        {
            Results = new WeeklyHoursView();
            using (var cn = Data.GetConnection())
            {
                // populate week selection dropdown going 3 weeks prior to the selected week
                Results.Weeks = await new CalendarWeeks() { Seed = DateTime.Today, WeeksBack = weeksBack + 3 }.ExecuteAsync(cn);

                WeekSelect = new SelectList(Results.Weeks.Select(wk => new SelectListItem() { Value = wk.WeekIndex.ToString(), Text = $"{wk.WeekNumber}: {wk.StartDate:M/d/yy} - {wk.EndDate:M/d/yy}" }), "Value", "Text", weeksBack);

                var weekDictionary = Results.Weeks.ToDictionary(row => row.WeekIndex);

                Results.WorkLogs = await new AllPendingWorkLogs()
                {
                    OrgId = OrgId,
                    UserId = UserId,
                    AppId = CurrentOrgUser.CurrentAppId,
                    WeekNumber = weekDictionary[weeksBack].WeekNumber,
                    Year = weekDictionary[weeksBack].Year
                }.ExecuteAsync(cn);
            }
        }
    }
}