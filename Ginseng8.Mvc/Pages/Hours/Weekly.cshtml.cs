using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Hours
{
	public class WeeklyModel : AppPageModel
	{
		public WeeklyModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<CalendarWeeksResult> Weeks { get; set; }
		public IEnumerable<PendingWorkLogsResult> WorkLogs { get; set; }

		public SelectList WeekSelect { get; set; }

		public IEnumerable<DateTime> GetColumns()
		{
			return WorkLogs.GroupBy(row => row.Date).Select(grp => grp.Key);
		}

		public async Task OnGetAsync(int weeksBack = 0)
		{			
			using (var cn = Data.GetConnection())
			{
				// populate week selection dropdown going 3 weeks prior to the selected week
				Weeks = await new CalendarWeeks() { Seed = DateTime.Today, Weeks = weeksBack + 3 }.ExecuteAsync(cn);

				WeekSelect = new SelectList(Weeks.Select(wk => new SelectListItem() { Value = wk.WeekIndex.ToString(), Text = $"{wk.WeekNumber}: {wk.StartDate:M/d/yy} - {wk.EndDate:M/d/yy}" }), "Value", "Text", weeksBack);

				var weekDictionary = Weeks.ToDictionary(row => row.WeekIndex);

				WorkLogs = await new PendingWorkLogs()
				{
					OrgId = OrgId,
					UserId = UserId,
					WeekNumber = weekDictionary[weeksBack].WeekNumber,
					Year = weekDictionary[weeksBack].Year
				}.ExecuteAsync(cn);
			}
		}
	}
}