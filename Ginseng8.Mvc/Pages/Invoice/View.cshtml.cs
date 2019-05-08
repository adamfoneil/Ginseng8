using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Invoice
{
    [Authorize]
    public class ViewModel : AppPageModel
    {
        public ViewModel(IConfiguration config) : base(config)
        {
        }

        public Ginseng.Models.Invoice Header { get; set; }
        public IEnumerable<WorkLogsResult> Details { get; set; }

        public async Task OnGetAsync(int id)
        {
            using (var cn = Data.GetConnection())
            {
                Header = await cn.FindAsync<Ginseng.Models.Invoice>(id);
                Details = await new AllInvoiceWorkLogs() { OrgId = OrgId, InvoiceId = id }.ExecuteAsync(cn);
            }
        }
    }
}