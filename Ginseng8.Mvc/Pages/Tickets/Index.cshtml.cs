﻿using Ginseng.Integration.Services;
using Ginseng.Models;
using Ginseng.Mvc.Extensions;
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
        private readonly IFreshdeskClientFactory _freshdeskClientFactory;

        public IndexModel(
            IConfiguration config,
            FreshdeskTicketCache ticketCache,
            FreshdeskGroupCache groupCache,
            IFreshdeskClientFactory freshdeskClientFactory)
            : base(config)
        {
            _ticketCache = ticketCache;
            _groupCache = groupCache;
            _freshdeskClientFactory = freshdeskClientFactory;
        }

        public IEnumerable<WorkItemTicket> IgnoredTickets { get; set; }
        public Dictionary<long, Group> Groups { get; set; }
        public IEnumerable<Ticket> Tickets { get; set; }
        public LoadedFrom LoadedFrom { get; set; }
        public DateTime DateQueried { get; set; }
        public string FreshdeskUrl { get; set; }

        public SelectList ActionSelect { get; set; }

        public async Task OnGetAsync()
        {            
            using (var cn = Data.GetConnection())
            {
                var appItems = await new AppSelect() { OrgId = OrgId }.ExecuteItemsAsync(cn);
                appItems.Insert(0, new SelectListItem() { Value = "0", Text = "Ignore Ticket" });
                ActionSelect = new SelectList(appItems, "Value", "Text");

                IgnoredTickets = await new AllWorkItemTickets() { OrgId = OrgId, IsIgnored = true }.ExecuteAsync(cn);
            }            

            FreshdeskUrl = Data.CurrentOrg.FreshdeskUrl;
            var tickets = await _ticketCache.QueryAsync(Data.CurrentOrg.Name);
            Tickets = tickets.Where(t => !IgnoredTickets.Any(it => it.TicketId == t.Id));

            var groups = await _groupCache.QueryAsync(Data.CurrentOrg.Name);
            Groups = groups.ToDictionary(row => row.Id);

            LoadedFrom = _ticketCache.LoadedFrom;
            DateQueried = _ticketCache.LastApiCallDateTime;
        }

        public async Task<IActionResult> OnPostDoActionAsync(int ticketId, int appId)
        {
            var client = await _freshdeskClientFactory.CreateClientForOrganizationAsync(OrgId);
            var ticket = await client.GetTicketAsync(ticketId);

            using (var cn = Data.GetConnection())
            {
                var wit =
                    await cn.FindWhereAsync<WorkItemTicket>(new { TicketId = ticketId }) ??
                    new WorkItemTicket()
                    {
                        TicketId = ticketId,
                        OrganizationId = OrgId,
                        TicketStatus = ticket.Status,
                        TicketType = WebhookRequestToWebhookConverter.TicketTypeFromString(ticket.Type)
                    };

                if (appId == 0)
                {
                    wit.WorkItemNumber = 0;
                }
                else
                {
                    int number = await CreateTicketWorkItemAsync(cn, ticket);
                    wit.WorkItemNumber = number;
                    await client.UpdateTicketWorkItemAsync(ticketId, number.ToString());
                }

                await Data.TrySaveAsync(cn, wit);
            }

            return Redirect("Tickets/Index");
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
    }
}