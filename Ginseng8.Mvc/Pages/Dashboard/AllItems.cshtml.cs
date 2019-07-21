using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Postulate.Base.Extensions;
using Postulate.SqlServer.IntKey;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Work
{
    [Authorize]
    public class AllItemsModel : DashboardPageModel, IPaged
    {
        public AllItemsModel(IConfiguration config) : base(config)
        {
            ShowLabelFilter = false;
        }

        [BindProperty(SupportsGet = true)]
        public string Query { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterPriorityGroupId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterUserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterProjectId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterMilestoneId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterSizeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterActivityId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterCloseReasonId { get; set; } = 0;

        [BindProperty(SupportsGet = true)]
        public int? FilterLabelId { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? FilterFreshdeskTickets { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? PastDue { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? PageNumber { get; set; } = 0;

        /// <summary>
        /// Projects found by a search
        /// </summary>
        public IEnumerable<ProjectInfoResult> Projects { get; set; }

        public Dictionary<PriorityGroupOptions, PriorityGroup> PriorityGroups { get; set; }

        public SelectList PriorityGroupSelect { get; set; }
        public SelectList UserSelect { get; set; }
        public SelectList ActivitySelect { get; set; }
        public SelectList CloseReasonSelect { get; set; }
        public SelectList LabelSelect { get; set; }

        protected override async Task<RedirectResult> GetRedirectAsync(SqlConnection connection)
        {
            if (int.TryParse(Query, out int number))
            {
                if (connection.Exists("[dbo].[WorkItem] WHERE [OrganizationId]=@orgId AND [Number]=@number", new { orgId = OrgId, Number = number }))
                {
                    return new RedirectResult($"/WorkItem/View/{number}");
                }
            }

            if (IsTicket(Query, out int ticket))
            {
                var wit = await connection.FindWhereAsync<WorkItemTicket>(new { OrganizationId = OrgId, TicketId = ticket });
                if (wit != null)
                {
                    return new RedirectResult($"/WorkItem/View/{wit.WorkItemNumber}");
                }
            }

            return await Task.FromResult<RedirectResult>(null);
        }

        private bool IsTicket(string query, out int ticketNumber)
        {
            ticketNumber = 0;
            if (string.IsNullOrEmpty(query)) return false;

            if (query.ToLower().StartsWith("fd"))
            {
                return int.TryParse(query.Substring(2), out ticketNumber);
            }
            return false;
        }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            var groups = await new PriorityGroups().ExecuteAsync(connection);
            PriorityGroups = groups.ToDictionary(row => (PriorityGroupOptions)row.Id);
            var groupItems = groups.Select(row => new SelectListItem() { Value = row.Id.ToString(), Text = row.Name });
            PriorityGroupSelect = new SelectList(groupItems, "Value", "Text", FilterPriorityGroupId);

            var userList = await new UserSelect() { OrgId = OrgId }.ExecuteItemsAsync(connection);
            userList.Insert(0, new SelectListItem() { Value = "0", Text = "- no assigned user -" });
            UserSelect = new SelectList(userList, "Value", "Text", FilterUserId);

            var activityList = await new ActivitySelect() { OrgId = OrgId }.ExecuteItemsAsync(connection);
            activityList.Insert(0, new SelectListItem() { Value = "0", Text = "- no current activity -" });
            ActivitySelect = new SelectList(activityList, "Value", "Text", FilterActivityId);

            var closeReasonList = await new CloseReasonSelect().ExecuteItemsAsync(connection);
            closeReasonList.Insert(0, new SelectListItem() { Value = "0", Text = "- open items -" });
            closeReasonList.Insert(1, new SelectListItem() { Value = "-1", Text = "- closed any reason -" });
            CloseReasonSelect = new SelectList(closeReasonList, "Value", "Text", FilterCloseReasonId);

            var labelList = await new OpenLabels() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection);
            var items = labelList.Select(row => new SelectListItem() { Value = row.Id.ToString(), Text = $"{row.Name}: {row.WorkItemCount}" });
            LabelSelect = new SelectList(items, "Value", "Text", FilterLabelId);

            if (!string.IsNullOrEmpty(Query))
            {
                // if a search was passed in, execute that on the project list
                Projects = await new ProjectInfo() { OrgId = OrgId, TitleAndBodySearch = Query, AppId = CurrentOrgUser.CurrentAppId, IsActive = true }.ExecuteAsync(connection);
            }
        }

        protected override OpenWorkItems GetQuery()
        {
            return new OpenWorkItems(QueryTraces)
            {
                IsOpen = null,
                OrgId = OrgId,
                TeamId = CurrentOrgUser.CurrentTeamId,
                AppId = CurrentOrgUser.EffectiveAppId,
                ProjectId = FilterProjectId,
                LabelId = FilterLabelId,
                MilestoneId = FilterMilestoneId,
                SizeId = FilterSizeId,
                TitleAndBodySearch = Query,
                IsPastDue = PastDue,
                AssignedUserId = FilterUserId,
                ActivityId = FilterActivityId,
                CloseReasonId = FilterCloseReasonId,
                PriorityGroupId = FilterPriorityGroupId,
                IsFreshdeskTicket = FilterFreshdeskTickets,
                Page = PageNumber,
            };
        }
    }
}