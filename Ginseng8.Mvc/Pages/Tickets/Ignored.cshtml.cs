using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Models.Freshdesk.Dto;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
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

        public async Task OnGetAsync(int responsibilityId = 0)
        {
            await InitializeAsync(responsibilityId);
            Tickets = FreshdeskCache.Tickets.Where(t => IgnoredTickets.Contains(t.Id));
        }
    }
}