using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Hours
{
    public class InvoiceModel : AppPageModel
    {
        public InvoiceModel(IConfiguration config) : base(config)
        {
        }

        public IEnumerable<PendingWorkLog> WorkLogs { get; set; }
        public SelectList ProjectSelect { get; set; }
        public SelectList AppSelect { get; set; }

        public async Task OnGetAsync()
        {
            using (var cn = Data.GetConnection())
            {
                WorkLogs = await new PendingWorkLogs()
                {
                    OrgId = OrgId,
                    AppId = CurrentOrgUser.CurrentAppId,
                    UserId = UserId
                }.ExecuteAsync(cn);                
            }
        }

        public async Task OnPostSaveAsync(PendingWorkLog record)
        {

        }
    }
}