﻿using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.Models;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Invoice
{
    public class ViewModel : AppPageModel
    {
        public ViewModel(IConfiguration config) : base(config)
        {
        }

        public Ginseng.Models.Invoice Header { get; set; }
        public IEnumerable<WorkLogsResult> Details { get; set; }
        public IEnumerable<CalendarWeeksResult> Weeks { get; set; }

        public async Task OnGetAsync(int id, int orgId)
        {
            using (var cn = Data.GetConnection())
            {
                Header = await cn.FindWhereAsync<Ginseng.Models.Invoice>(new { OrganizationId = orgId, Number = id });
                Details = await new AllInvoiceWorkLogs() { OrgId = orgId, InvoiceId = Header.Id }.ExecuteAsync(cn);
                if (Details.Any())
                {
                    Weeks = await new CalendarWeeks()
                    {
                        Seed = Details.Max(row => row.Date),
                        WeeksBack = Details.GroupBy(row => row.ToWeek()).Count()
                    }.ExecuteAsync(cn);
                }
            }
        }
    }
}