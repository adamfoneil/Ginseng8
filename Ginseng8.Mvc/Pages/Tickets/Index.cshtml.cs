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
    /// <summary>
    /// What kind of items are in the action dropdown?
    /// This depends on whether the current team uses applications and whether a current app is selected
    /// </summary>
    public enum ActionItemType
    {
        /// <summary>
        /// For teams that don't use apps or if team *does* use apps and one is selected, we show projects
        /// </summary>
        Project,

        /// <summary>
        /// If no current app selected, then we show apps
        /// </summary>
        Application
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

        public ActionItemType ActionObjectType { get; set; } // tells us whether the work item action Id is an app or a project
        public SelectList AppSelect { get; set; } // available when no current app selected
        public Dictionary<long, SelectList> ProjectByCompanySelect { get; set; } // available when app is current

        public SelectList GetActionSelect(long companyId)
        {
            return (ActionObjectType == ActionItemType.Project && ProjectByCompanySelect.ContainsKey(companyId)) ? ProjectByCompanySelect[companyId] : AppSelect;
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

            int teamId = CurrentOrgUser.CurrentTeamId ?? 0;

            using (var cn = Data.GetConnection())
            {
                AppSelect = await BuildAppSelectAsync(cn);

                var assignedTickets = await new AssignedTickets() { OrgId = OrgId }.ExecuteAsync(cn);
                Tickets = FreshdeskCache.Tickets.Where(t => !IgnoredTickets.Contains(t.Id) && !assignedTickets.Contains(t.Id)).ToArray();

                if (CurrentOrgUser.CurrentTeamId.HasValue)
                {
                    ActionObjectType = (!CurrentOrgUser.CurrentTeam.UseApplications || (CurrentOrgUser.CurrentAppId.HasValue && CurrentOrgUser.CurrentTeam.UseApplications)) ? ActionItemType.Project : ActionItemType.Application;
                    ProjectByCompanySelect = await BuildProjectSelectAsync(cn, teamId, CurrentOrgUser.CurrentAppId, Tickets);
                }
            }

            LoadedFrom = FreshdeskCache.TicketCache.LoadedFrom;
            DateQueried = FreshdeskCache.TicketCache.LastApiCallDateTime;
        }

        /// <summary>
        /// Returns a dictionary of SeletLists keyed to Freshdesk Company Ids
        /// </summary>
        private async Task<Dictionary<long, SelectList>> BuildProjectSelectAsync(SqlConnection cn, int teamId, int? appId, IEnumerable<Ticket> tickets)
        {
            Dictionary<long, SelectList> results = new Dictionary<long, SelectList>();

            var projects = await new ProjectSelectEx() { OrgId = OrgId, TeamId = teamId, AppId = appId }.ExecuteAsync(cn);

            var team = await cn.FindAsync<Team>(teamId);
            bool companySpecific = team?.CompanySpecificProjects ?? false;

            if (companySpecific)
            {
                var projectsByCompany = projects.GroupBy(row => row.FreshdeskCompanyId ?? 0);
                foreach (var companyGrp in projectsByCompany)
                {
                    var items = companyGrp.ToList();
                    if (teamId != 0) items.Insert(0, new ProjectSelectResult() { Value = 0, Text = "Ignore Ticket" });
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
                    if (teamId != 0) items.Add(new SelectListItem() { Value = "0", Text = "Ignore Ticket" });
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

        private async Task<SelectList> BuildAppSelectAsync(SqlConnection cn)
        {
            var appItems = await new AppSelect() { OrgId = OrgId }.ExecuteItemsAsync(cn);
            // you can ignore tickets only if a team is selected
            if (CurrentOrgUser.CurrentTeamId.HasValue) appItems.Insert(0, new SelectListItem() { Value = "0", Text = "Ignore Ticket" });
            return new SelectList(appItems, "Value", "Text");
        }

        private Task<List<SelectListItem>> GetProjectSelectItems(SqlConnection cn)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> OnPostDoActionAsync(int ticketId, int objectId, int teamId, ActionItemType objectType)
        {
            var client = await _freshdeskClientFactory.CreateClientForOrganizationAsync(OrgId);
            var ticket = await client.GetTicketAsync(ticketId, withConversations: true);

            await FreshdeskCache.InitializeAsync(OrgName);

            using (var cn = Data.GetConnection())
            {
                if (objectId == 0)
                {
                    await IgnoreTicketInner(ticketId, teamId, cn);
                }
                else
                {
                    int number = await CreateWorkItemFromTicketAsync(cn, client, ticket, objectId, objectType);
                    await FreshdeskCache.LinkWorkItemToTicketAsync(cn, client, OrgId, number, ticket, CurrentUser);                    
                }
            }

            return Redirect($"Tickets/Index");
        }

        private async Task IgnoreTicketInner(int ticketId, int teamId, SqlConnection cn)
        {
            var ignoreTicket = new IgnoredTicket()
            {
                TicketId = ticketId,
                OrganizationId = OrgId,
                TeamId = teamId
            };
            await Data.TrySaveAsync(cn, ignoreTicket);
        }

        private async Task<int> CreateWorkItemFromTicketAsync(SqlConnection cn, IFreshdeskClient client, Ticket ticket, int objectId, ActionItemType objectType)
        {
            int? appId = (objectType == ActionItemType.Application) ?
                objectId :
                (objectId == -1) ?
                    CurrentOrgUser.CurrentAppId :
                    await GetAppFromProjectIdAsync(cn, objectId);

            int? projectId = (objectType == ActionItemType.Project) ? objectId : default(int?);

            if (projectId == -1 && ticket.CompanyId.HasValue)
            {
                var company = await client.GetCompanyAsync(ticket.CompanyId.Value);
                projectId = await CreateNewProjectAsync(cn, CurrentOrgUser.CurrentAppId.Value, company);
            }

            if (projectId == -1) projectId = null;            

            var workItem = new Ginseng.Models.WorkItem()
            {
                OrganizationId = OrgId,
                ApplicationId = appId,
                TeamId = CurrentOrgUser.CurrentTeamId.Value,
                ProjectId = projectId,
                Title = $"FD: {ticket.Subject} (ticket # {ticket.Id})",
                HtmlBody = ticket.Description + $"<hr/><p>Created from Freshdesk <a href=\"{CurrentOrg.FreshdeskUrl}/a/tickets/{ticket.Id}\">ticket {ticket.Id}</a></p>"
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

        private async Task<int> CreateNewProjectAsync(SqlConnection cn, int? appId, Company company)
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
                TeamId = CurrentOrgUser.CurrentTeamId.Value,
                ApplicationId = appId,
                Name = projectName,
                FreshdeskCompanyId = company.Id
            };

            await Data.TrySaveAsync(prj);

            return prj.Id;
        }

        private async Task<int?> GetAppFromProjectIdAsync(SqlConnection cn, int projectId)
        {
            var prj = await cn.FindAsync<Project>(projectId);
            return prj.ApplicationId;
        }

        public async Task<RedirectResult> OnPostIgnoreSelectedAsync(string ticketIds, int teamId)
        {
            int[] tickets = ticketIds.Split(',').Select(s => int.Parse(s)).ToArray();
            using (var cn = Data.GetConnection())
            {
                foreach (int ticketId in tickets) await IgnoreTicketInner(ticketId, teamId, cn);
            }
            return Redirect($"Tickets/Index");
        }
    }
}