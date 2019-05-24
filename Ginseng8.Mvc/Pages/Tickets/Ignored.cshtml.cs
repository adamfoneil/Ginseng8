﻿using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Tickets
{
    public class IgnoredModel : TicketPageModel
    {
        public IgnoredModel(
            IConfiguration config,
            IFreshdeskClientFactory freshdeskClientFactory) : base(config, freshdeskClientFactory)
        {
        }

        public async Task OnGetAsync()
        {
            await InitializeAsync();
            Tickets = FreshdeskCache.Tickets.Where(t => IgnoredTickets.Contains(t.Id));
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