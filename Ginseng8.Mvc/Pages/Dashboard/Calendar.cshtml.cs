using Ginseng.Mvc.Classes;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Dashboard
{
    [Authorize]
    public class CalendarModel : AppPageModel
    {
        public CalendarModel(IConfiguration config) : base(config)
        {
        }

        public IEnumerable<YearMonth> MonthCells { get; set; }
        public ILookup<YearMonth, CalendarProjectsResult> Projects { get; set; }

        public bool ShowTeams
        {
            get { return !CurrentOrgUser.CurrentTeamId.HasValue && !CurrentOrgUser.CurrentAppId.HasValue; }
        }

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                var projects = await new CalendarProjects()
                {
                    OrgId = OrgId,
                    TeamId = CurrentOrgUser.CurrentTeamId,
                    AppId = CurrentOrgUser.CurrentAppId
                }.ExecuteAsync(cn);
                Projects = projects.ToLookup(row => new YearMonth(row.Year, row.Month));

                MonthCells = AppendMonths(Projects.Select(grp => grp.Key), 4);
            }
        }

        private IEnumerable<YearMonth> AppendMonths(IEnumerable<YearMonth> months, int count)
        {
            if (!months.Any())
            {
                var start = new YearMonth();
                return Enumerable.Range(1, count).Select(i => start + i);
            }

            var last = months.Last();
            var list = months.ToList();
            list.AddRange(Enumerable.Range(1, count).Select(i => last + i));
            return list;
        }
    }
}