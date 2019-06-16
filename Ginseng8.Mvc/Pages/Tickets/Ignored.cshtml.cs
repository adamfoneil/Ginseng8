using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Tickets
{
    public class IgnoredModel : TicketPageModel, IPaged
    {
        public IgnoredModel(
            IConfiguration config,
            IFreshdeskClientFactory freshdeskClientFactory) : base(config, freshdeskClientFactory)
        {
        }

        [BindProperty(SupportsGet = true)]
        public int? PageNumber { get; set; } = 0;

        public async Task OnGetAsync()
        {
            await InitializeAsync();
            Tickets = from t in FreshdeskCache.Tickets
                      join i in IgnoredTickets on t.Id equals i
                      orderby t.CreatedAt descending
                      select t;
        }

        protected override async Task<IEnumerable<long>> GetIgnoredTicketsAsync(IDbConnection connection)
        {
            return await new IgnoredTickets() { TeamId = ResponsibilityId, OrgId = OrgId, Page = PageNumber }.ExecuteAsync(connection);
        }

        public async Task<RedirectResult> OnPostRestoreAsync(long ticketId, int responsibilityId)
        {
            using (var cn = Data.GetConnection())
            {
                var ignored = await cn.FindWhereAsync<IgnoredTicket>(new { OrganizationId = OrgId, TicketId = ticketId, ResponsibilityId = responsibilityId });
                if (ignored != null) await cn.DeleteAsync<IgnoredTicket>(ignored.Id);
            }

            return Redirect($"Ignored?responsibilityId={responsibilityId}");
        }
    }
}