using Ginseng.Models;
using Ginseng.Mvc.Helpers;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Mapping;
using Ginseng.Mvc.Models.Freshdesk.Dto;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Ginseng.Mvc.Services;
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

namespace Ginseng.Mvc.Pages.Tickets
{
    [Authorize]
    public class IndexModel : AppPageModel
    {
        private readonly FreshdeskTicketCache _ticketCache;
        private readonly FreshdeskGroupCache _groupCache;
        private readonly FreshdeskContactCache _contactCache;
        private readonly FreshdeskCompanyCache _companyCache;
        private readonly IFreshdeskClientFactory _freshdeskClientFactory;

        public IndexModel(
            IConfiguration config,
            FreshdeskTicketCache ticketCache,
            FreshdeskGroupCache groupCache,
            FreshdeskCompanyCache companyCache,
            FreshdeskContactCache contactCache,
            IFreshdeskClientFactory freshdeskClientFactory)
            : base(config)
        {
            _ticketCache = ticketCache;
            _groupCache = groupCache;
            _contactCache = contactCache;
            _companyCache = companyCache;
            
            _freshdeskClientFactory = freshdeskClientFactory;
        }

        public Dictionary<string, string> TypeBadges
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "Issue", "badge-danger" },
                    { "Feature Request", "badge-info" },
                    { "Request", "badge-warning" }
                };
            }
        }

        public string GetTypeBadge(string type)
        {
            return (TypeBadges.ContainsKey(type)) ? TypeBadges[type] : "badge-light";
        }
        
        [BindProperty(SupportsGet = true)]
        public int ResponsibilityId { get; set; }

        public Dictionary<long, Group> Groups { get; set; }
        public Dictionary<long, Contact> Contacts { get; set; }
        public Dictionary<long, Company> Companies { get; set; }
        public IEnumerable<Ticket> Tickets { get; set; }
        public LoadedFrom LoadedFrom { get; set; }
        public DateTime DateQueried { get; set; }
        public string FreshdeskUrl { get; set; }

        public SelectList ActionSelect { get; set; }
        public SelectList ResponsibilitySelect { get; set; }

        public string GetContactName(long requesterId)
        {
            return (Contacts.ContainsKey(requesterId)) ? Contacts[requesterId].Name : $"requester id {requesterId}";
        }

        public string GetCompanyName(long companyId)
        {
            return (Companies.ContainsKey(companyId)) ? Companies[companyId].Name : $"company id {companyId}";
        }

        public async Task OnGetAsync(int responsibilityId = 0)
        {
            FreshdeskUrl = Data.CurrentOrg.FreshdeskUrl;
            var tickets = await _ticketCache.QueryAsync(Data.CurrentOrg.Name);

            var contacts = await _contactCache.QueryAsync(Data.CurrentOrg.Name);
            Contacts = contacts.ToDictionary(row => row.Id);

            var companies = await _companyCache.QueryAsync(Data.CurrentOrg.Name);
            Companies = companies.ToDictionary(row => row.Id);

            using (var cn = Data.GetConnection())
            {
                var appItems = await new AppSelect() { OrgId = OrgId }.ExecuteItemsAsync(cn);

                // you can ignore tickets only if a responsibility is selected
                if (responsibilityId != 0) appItems.Insert(0, new SelectListItem() { Value = "0", Text = "Ignore Ticket" });
                
                ActionSelect = new SelectList(appItems, "Value", "Text");

                ResponsibilitySelect = await new ResponsibilitySelect().ExecuteSelectListAsync(cn, responsibilityId);

                var ignoredTickets = await new IgnoredTickets() { ResponsibilityId = responsibilityId, OrgId = OrgId }.ExecuteAsync(cn);
                var assignedTickets = await new AssignedTickets() { OrgId = OrgId }.ExecuteAsync(cn);
                Tickets = tickets.Where(t => !ignoredTickets.Contains(t.Id) && !assignedTickets.Contains(t.Id));
            }
                        
            var groups = await _groupCache.QueryAsync(Data.CurrentOrg.Name);
            Groups = groups.ToDictionary(row => row.Id);

            LoadedFrom = _ticketCache.LoadedFrom;
            DateQueried = _ticketCache.LastApiCallDateTime;
        }

        public async Task<IActionResult> OnPostDoActionAsync(int ticketId, int appId, int responsibilityId)
        {
            var client = await _freshdeskClientFactory.CreateClientForOrganizationAsync(OrgId);
            var ticket = await client.GetTicketAsync(ticketId);

            using (var cn = Data.GetConnection())
            {                
                if (appId == 0)
                {
                    await IgnoreTicketInner(ticketId, responsibilityId, cn);
                }
                else
                {
                    int number = await CreateTicketWorkItemAsync(cn, ticket);
                    var wit = new WorkItemTicket()
                    {
                        TicketId = ticketId,
                        WorkItemNumber = number,
                        OrganizationId = OrgId,
                        TicketStatus = ticket.Status,
                        TicketType = WebhookRequestToWebhookConverter.TicketTypeFromString(ticket.Type)
                    };
                    
                    await client.UpdateTicketWorkItemAsync(ticketId, number.ToString());
                    await Data.TrySaveAsync(cn, wit);
                }                
            }

            return Redirect($"Tickets/Index?responsibilityId={responsibilityId}");
        }

        private async Task IgnoreTicketInner(int ticketId, int responsibilityId, SqlConnection cn)
        {
            var ignoreTicket = new IgnoredTicket()
            {
                TicketId = ticketId,
                OrganizationId = OrgId,
                ResponsibilityId = responsibilityId
            };
            await Data.TrySaveAsync(cn, ignoreTicket);
        }

        private async Task<int> CreateTicketWorkItemAsync(SqlConnection cn, Ticket ticket)
        {
            var workItem = new Ginseng.Models.WorkItem()
            {
                OrganizationId = OrgId,
                ApplicationId = CurrentOrgUser.CurrentAppId.Value,
                Title = $"FD: {ticket.Subject} (ticket # {ticket.Id})",
                HtmlBody = $"<p>Created from Freshdesk <a href=\"{CurrentOrg.FreshdeskUrl}/a/tickets/{ticket.Id}\">ticket {ticket.Id}</a></p>"
            };
            await workItem.SetNumberAsync(cn);
            await workItem.SaveHtmlAsync(Data, cn);
            await Data.TrySaveAsync(workItem);

            // this will make it "work on next" if it has no priority already
            var wip =
                await cn.FindWhereAsync<WorkItemPriority>(new { WorkItemId = workItem.Id }) ??
                new WorkItemPriority() { WorkItemId = workItem.Id, Value = 1 };

            await Data.TrySaveAsync(wip);

            return workItem.Number;
        }

        public async Task<RedirectResult> OnPostIgnoreSelectedAsync(string ticketIds, int responsibilityId)
        {
            int[] tickets = ticketIds.Split(',').Select(s => int.Parse(s)).ToArray();
            using (var cn = Data.GetConnection())
            {
                foreach (int ticketId in tickets) await IgnoreTicketInner(ticketId, responsibilityId, cn);
            }
            return Redirect($"Tickets/Index?responsibilityId={responsibilityId}");
        }
    }
}