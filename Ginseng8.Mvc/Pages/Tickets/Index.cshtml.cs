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
    public enum ActionObjectType
    {
        Project,
        Team
    }

    [Authorize]
    public class IndexModel : TicketPageModel
    {
        private IFreshdeskClientFactory _freshdeskClientFactory;

        public IndexModel(
            IConfiguration config,            
            IFreshdeskClientFactory freshdeskClientFactory)
            : base(config, freshdeskClientFactory)
        {
            _freshdeskClientFactory = freshdeskClientFactory;
        }

        public LoadedFrom LoadedFrom { get; set; }
        public DateTime DateQueried { get; set; }
       
        public ActionObjectType ActionObjectType { get; set; } // tells us whether the work item action Id is an app or a project
        public SelectList AppSelect { get; set; } // available when no current app selected
        public Dictionary<long, SelectList> ProjectByCompanySelect { get; set; } // available when app is current                        

        public SelectList GetActionSelect(long companyId)
        {
            return (CurrentOrgUser.CurrentAppId.HasValue && ProjectByCompanySelect.ContainsKey(companyId)) ? ProjectByCompanySelect[companyId] : AppSelect;
        }

        public string GetWorkItemCreateMessage()
        {
            return (CurrentOrgUser.CurrentAppId.HasValue) ?
                $"Select project in {CurrentOrgUser.CurrentApp.Name}" :
                "Select app";
        }

        public async Task OnGetAsync()
        {
            await InitializeAsync();

            using (var cn = Data.GetConnection())
            {                                
                var assignedTickets = await new AssignedTickets() { OrgId = OrgId }.ExecuteAsync(cn);                
                Tickets = FreshdeskCache.Tickets.Where(t => !IgnoredTickets.Contains(t.Id) && !assignedTickets.Contains(t.Id)).ToArray();

                if (CurrentOrgUser.CurrentAppId.HasValue)
                {
                    ActionObjectType = ActionObjectType.Project;
                    ProjectByCompanySelect = await BuildProjectSelectAsync(cn, ResponsibilityId, CurrentOrgUser.CurrentAppId.Value, Tickets);
                }
                else
                {
                    ActionObjectType = ActionObjectType.Team;
                    AppSelect = await BuildAppSelectAsync(cn, ResponsibilityId);
                }
            }            

            LoadedFrom = FreshdeskCache.TicketCache.LoadedFrom;
            DateQueried = FreshdeskCache.TicketCache.LastApiCallDateTime;
        }

        /// <summary>
        /// Returns a dictionary of SeletLists keyed to Freshdesk Company Ids
        /// </summary>
        private async Task<Dictionary<long, SelectList>> BuildProjectSelectAsync(SqlConnection cn, int responsibilityId, int appId, IEnumerable<Ticket> tickets)
        {
            Dictionary<long, SelectList> results = new Dictionary<long, SelectList>();

            var projects = await new ProjectSelectEx() { OrgId = OrgId, AppId = appId }.ExecuteAsync(cn);

            var resp = await cn.FindAsync<Responsibility>(responsibilityId);
            bool companySpecific = resp?.CompanySpecificProjects ?? false;

            if (companySpecific)
            {                
                var projectsByCompany = projects.GroupBy(row => row.FreshdeskCompanyId ?? 0);
                foreach (var companyGrp in projectsByCompany)
                {
                    var items = companyGrp.ToList();
                    if (responsibilityId != 0) items.Insert(0, new ProjectSelectResult() { Value = 0, Text = "Ignore Ticket" });
                    items.Insert(1, new ProjectSelectResult() { Value = -1, Text = "[ new project ]" });
                    var selectItems = items.Select(item => new SelectListItem() { Value = item.Value.ToString(), Text = item.Text });
                    var firstProject = items.FirstOrDefault(p => p.FreshdeskCompanyId == companyGrp.Key);
                    results.Add(companyGrp.Key, new SelectList(selectItems, "Value", "Text", firstProject?.Value));
                }
            }

            var ticketsByCompany = tickets.GroupBy(row => row.CompanyId ?? 0);
            foreach (var companyGrp in ticketsByCompany)
            {
                if (!results.ContainsKey(companyGrp.Key))
                {
                    var items = new List<SelectListItem>();
                    if (responsibilityId != 0) items.Add(new SelectListItem() { Value = "0", Text = "Ignore Ticket" });
                    items.Add(new SelectListItem() { Value = "-1", Text = "[ new project ]" });
                    if (!companySpecific)
                    {
                        items.AddRange(projects.Where(p => (p.FreshdeskCompanyId ?? 0) == 0).Select(p => new SelectListItem() { Value = p.Value.ToString(), Text = p.Text }));
                    }
                    results.Add(companyGrp.Key, new SelectList(items, "Value", "Text"));
                }
            }
            return results;
        }

        private async Task<SelectList> BuildAppSelectAsync(SqlConnection cn, int responsibilityId)
        {
            var appItems = await new AppSelect() { OrgId = OrgId }.ExecuteItemsAsync(cn);
            // you can ignore tickets only if a responsibility is selected
            if (responsibilityId != 0) appItems.Insert(0, new SelectListItem() { Value = "0", Text = "Ignore Ticket" });
            return new SelectList(appItems, "Value", "Text");
        }

        private Task<List<SelectListItem>> GetProjectSelectItems(SqlConnection cn)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> OnPostDoActionAsync(int ticketId, int objectId, int responsibilityId, ActionObjectType objectType)
        {
            var client = await _freshdeskClientFactory.CreateClientForOrganizationAsync(OrgId);
            var ticket = await client.GetTicketAsync(ticketId);

            await FreshdeskCache.InitializeAsync(OrgName);

            using (var cn = Data.GetConnection())
            {                
                if (objectId == 0)
                {
                    await IgnoreTicketInner(ticketId, responsibilityId, cn);
                }
                else
                {
                    int number = await CreateWorkItemFromTicketAsync(cn, client, ticket, objectId, objectType);
                    var wit = new WorkItemTicket()
                    {
                        TicketId = ticketId,
                        WorkItemNumber = number,
                        OrganizationId = OrgId,
                        TicketStatus = ticket.Status,
                        TicketType = WebhookRequestToWebhookConverter.TicketTypeFromString(ticket.Type),
                        CompanyId = ticket.CompanyId,
                        CompanyName = GetCompanyName(ticket.CompanyId ?? 0),
                        ContactId = ticket.RequesterId,
                        ContactName = GetContactName(ticket.RequesterId),
                        Subject = ticket.Subject
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

        private async Task<int> CreateWorkItemFromTicketAsync(SqlConnection cn, IFreshdeskClient client, Ticket ticket, int objectId, ActionObjectType objectType)
        {
            int teamId = (objectType == ActionObjectType.Team) ? 
                objectId : 
                (objectId == -1) ?
                    CurrentOrgUser.CurrentAppId.Value :
                    await GetAppFromProjectIdAsync(cn, objectId);

            int? projectId = (objectType == ActionObjectType.Project) ? objectId : default(int?);

            if (projectId == -1 && ticket.CompanyId.HasValue)
            {                                
                var company = await client.GetCompanyAsync(ticket.CompanyId.Value);
                projectId = await CreateNewProjectAsync(cn, CurrentOrgUser.CurrentAppId.Value, company);
            }

            if (projectId == -1) projectId = null;

            var workItem = new Ginseng.Models.WorkItem()
            {
                OrganizationId = OrgId,
                TeamId = teamId,
                ProjectId = projectId,
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

        private async Task<int> CreateNewProjectAsync(SqlConnection cn, int appId, Company company)
        {
            string projectName = company.Name;
            int increment = 0;
            while (await cn.ExistsWhereAsync<Project>(new { ApplicationId = appId, Name = projectName }))
            {
                increment++;
                projectName = company.Name + increment.ToString();
            }

            var prj = new Project()
            {
                ApplicationId = appId,
                Name = projectName,
                FreshdeskCompanyId = company.Id
            };

            await Data.TrySaveAsync(prj);

            return prj.Id;
        }

        private async Task<int> GetAppFromProjectIdAsync(SqlConnection cn, int projectId)
        {
            var prj = await cn.FindAsync<Project>(projectId);
            return prj.ApplicationId ?? 0;
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