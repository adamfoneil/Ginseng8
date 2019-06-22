using Ginseng.Models;
using Ginseng.Mvc.Classes;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                var months = await new MilestoneMonths()
                {
                    OrgId = OrgId,
                    TeamId = CurrentOrgUser.CurrentTeamId,
                    AppId = CurrentOrgUser.CurrentAppId
                }.ExecuteAsync(cn);

                MonthCells = AppendMonths(months, 4);
            }
        }


        private IEnumerable<YearMonth> AppendMonths(IEnumerable<YearMonth> months, int count)
        {
            var last = months.Last();
            var list = months.ToList();
            list.AddRange(Enumerable.Range(1, count).Select(i => last + i));
            return list;
        }

        private IEnumerable<YearMonth> GetYearMonthRange(YearMonth start, YearMonth end)
        {
            List<YearMonth> results = new List<YearMonth>();
            YearMonth add = start;
            while (add <= end)
            {
                results.Add(add);
                add += 1;
            }
            return results;
        }
    }
}