using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ginseng.Mvc.Models.Freshdesk.Dto;
using System.Linq;
using Ginseng.Mvc.Queries;
using Ginseng.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ginseng.Mvc.Extensions;

namespace Ginseng.Mvc.Pages.Tickets
{
    public enum TicketAction
    {
        Ignore,
        CreateWorkItem
    }

	[Authorize]
	public class IndexModel : AppPageModel
    {
        private readonly FreshdeskTicketCache _cache;

		public IndexModel(
            IConfiguration config,
            FreshdeskTicketCache cache) 
            : base(config)
        {
            _cache = cache;
        }

		public IEnumerable<Ticket> Tickets { get; set; }
		public LoadedFrom LoadedFrom { get; set; }
		public DateTime DateQueried { get; set; }
        public string FreshdeskUrl { get; set; }

        public SelectList ActionSelect { get; set; }

		public async Task OnGetAsync()
		{
            ActionSelect = SelectListHelper.FromEnum<TicketAction>();

            var ignoredTickets = Enumerable.Empty<IgnoredTicket>();
            using (var cn = Data.GetConnection())
            {
                ignoredTickets = await new AllIgnoredTickets() { OrgId = OrgId }.ExecuteAsync(cn);
            }

            FreshdeskUrl = Data.CurrentOrg.FreshdeskUrl;
            var tickets = await _cache.QueryAsync(Data.CurrentOrg.Name);

            Tickets = tickets.Where(t => !ignoredTickets.Any(it => it.TicketId == t.Id));
			LoadedFrom = _cache.LoadedFrom;
			DateQueried = _cache.LastApiCallDateTime;
		}

        public async Task OnPostDoActionAsync(int ticketId, TicketAction action)
        {
            using (var cn = Data.GetConnection())
            {
                switch (action)
                {
                    case TicketAction.Ignore:
                        await IgnoreTicketAsync(ticketId);
                        break;

                    case TicketAction.CreateWorkItem:
                        await CreateWorkItemAsync(ticketId);
                        break;
                }
            }                
        }

        private Task CreateWorkItemAsync(int ticketId)
        {
            throw new NotImplementedException();
        }

        private Task IgnoreTicketAsync(int ticketId)
        {
            throw new NotImplementedException();
        }
    }
}