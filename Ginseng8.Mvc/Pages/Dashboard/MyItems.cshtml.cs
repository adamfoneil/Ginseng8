using Dapper;
using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Dashboard
{
    [Authorize]
    public class MyItemsModel : DashboardPageModel
    {
        public MyItemsModel(IConfiguration config) : base(config)
        {
        }

        /// <summary>
        /// What column is user Id assigned to when items are created?
        /// </summary>
        public string UserIdColumnName { get; set; }

        /// <summary>
        /// All my hand-off activities
        /// </summary>
        public IEnumerable<ActivitySubscription> MyActivitySubscriptions { get; set; }

        /// <summary>
        /// Items in activities that I follow that are paused
        /// </summary>
        public IEnumerable<OpenWorkItemsResult> MyHandOffItems { get; set; }

        public ILookup<int, Label> HandOffLabels { get; set; }
        public ILookup<int, Comment> HandOffComments { get; set; }
        public IEnumerable<MyWorkScheduleResult> MySchedule { get; set; }
        public int UnestimatedItemCount { get; set; }
        public IEnumerable<Milestone> HiddenMilestones { get; set; }
        public IEnumerable<OpenWorkItemsResult> PinnedItems { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? Date { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool NeedsEstimate { get; set; }

        public HtmlString MyHandOffActivityList()
        {
            string result = (MyActivitySubscriptions?.Any() ?? false) ?
                string.Join(", ", MyActivitySubscriptions
                    .GroupBy(asub => asub.ActivityId)
                    .Select(grp => $"<strong>{grp.First().ActivityName}:</strong> {string.Join(", ", grp.Select(asub => asub.AppName))}")) :
                "none";

            return new HtmlString(result);
        }

        protected override async Task InitializeAsync(SqlConnection connection)
        {
            await connection.ExecuteAsync(
                @"INSERT INTO [dbo].[MilestoneUserView] (
                    [MilestoneId], [UserId], [IsVisible], [DateCreated], [CreatedBy]
                ) SELECT 
                    [ms].[Id], @userId, 1, @localDate, @userName
                FROM 
                    [dbo].[Milestone] [ms]
                WHERE 
                    [ms].[OrganizationId]=@orgId AND 
                    NOT EXISTS(SELECT 1 FROM [dbo].[MilestoneUserView] WHERE [MilestoneId]=[ms].[Id] AND [UserId]=@userId)",
                new { orgId = OrgId, userId = UserId, CurrentUser.UserName, localDate = CurrentUser.LocalTime });

            await connection.ExecuteAsync(
                @"INSERT INTO [dbo].[MilestoneUserView] (
                    [MilestoneId], [UserId], [IsVisible], [DateCreated], [CreatedBy]
                ) SELECT 
                    0, @userId, 1, @localDate, @userName
                FROM
                    [dbo].[FnIntRange](0, 0)
                WHERE
                    NOT EXISTS(SELECT 1 FROM [dbo].[MilestoneUserView] WHERE [MilestoneId]=0 AND [UserId]=@userId)",
                new { userId = UserId, CurrentUser.UserName, localDate = CurrentUser.LocalTime });
        }

        protected override OpenWorkItems GetQuery()
        {
            var result = new OpenWorkItems(QueryTraces)
            {
                OrgId = OrgId,                
                TeamId = CurrentOrgUser.CurrentTeamId,                
                LabelId = LabelId,
                VisibleToUserId = UserId,
                VisibleMilestones = true,
                PinnedItems = false
            };

            if (Options[Option.MyItemsFilterCurrentApp]?.BoolValue ?? true)
            {
                result.AppId = CurrentOrgUser.EffectiveAppId;
            }

            if (Options[Option.MyItemsGroupField]?.StringValue.Equals(MyItemGroupingOption.ActivityId) ?? false)
            {
                result.ActivityUserId = UserId;
            }

            var userIdField = Options[Option.MyItemsUserIdField].Value as string;
            UserIdFieldOption = MyItemUserIdFieldOption[userIdField];
            UserIdFieldOption.Criteria.Invoke(result, UserId);

            if (Date.HasValue)
            {
                result.WithWorkSchedule = true;
                result.ScheduleUserId = UserId;
                result.WorkDate = Date;
            }

            if (NeedsEstimate) result.SizeId = 0;

            return result;
        }

        protected override void OnGetInternal(SqlConnection connection)
        {
            int responsibilityId = CurrentOrgUser.Responsibilities;
            // if you have dev and biz responsibility, then assume dev
            if (responsibilityId == 3 || responsibilityId == 0) responsibilityId = 2;
            UserIdColumnName = Responsibility.WorkItemColumnName[responsibilityId];

            var grouping = Options[Option.MyItemsGroupField].Value as string;
            GroupingOption = MyItemGroupingOptions[grouping];            
        }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            UnestimatedItemCount = WorkItems.Where(wi => wi.EstimateHours == 0).Count();

            MyActivitySubscriptions = await new MyHandOffActivities() { OrgId = OrgId, UserId = UserId }.ExecuteAsync(connection);
            MyHandOffItems = await new OpenWorkItems(QueryTraces) { OrgId = OrgId, InMyActivities = true, ActivityUserId = UserId, IsPaused = true, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection);

            var itemIds = MyHandOffItems.Select(wi => wi.Id).ToArray();
            var labelsInUse = await new LabelsInUse() { WorkItemIds = itemIds, OrgId = OrgId }.ExecuteAsync(connection);
            HandOffLabels = labelsInUse.ToLookup(row => row.WorkItemId);

            var comments = await new Comments() { OrgId = OrgId, ObjectIds = itemIds, ObjectType = ObjectType.WorkItem }.ExecuteAsync(connection);
            HandOffComments = comments.ToLookup(row => row.ObjectId);

            MySchedule = await new MyWorkSchedule() { OrgId = OrgId, UserId = UserId }.ExecuteAsync(connection);
            HiddenMilestones = await new HiddenMilestones() { OrgId = OrgId, UserId = UserId }.ExecuteAsync(connection);

            var pinnedItemQry = new OpenWorkItems() { OrgId = OrgId, PinnedItems = true };
            UserIdFieldOption.Criteria.Invoke(pinnedItemQry, UserId);
            PinnedItems = await pinnedItemQry.ExecuteAsync(connection);
        }

        public async Task<RedirectResult> OnPostSetOptionsAsync()
        {
            string[] fields = new string[]
            {
                Option.MyItemsFilterCurrentApp,
                Option.MyItemsGroupField,
                Option.MyItemsUserIdField
            };

            using (var cn = Data.GetConnection())
            {
                foreach (var field in fields)
                {
                    var option = await Option.FindByName(cn, field);
                    var uov = new UserOptionValue()
                    {
                        UserId = UserId,
                        OptionId = option.Id,
                        OptionType = option.OptionType,
                        Value = Request.Form[field].First()
                    };
                    await cn.MergeAsync(uov, CurrentUser);
                }
            }

            return Redirect("/Dashboard/MyItems");
        }

        public async Task<RedirectResult> OnPostHideMilestoneAsync(int id)
        {
            return await ShowHideMilestoneAsync(id, false);
        }

        public async Task<RedirectResult> OnPostShowMilestoneAsync(int id)
        {
            return await ShowHideMilestoneAsync(id, true);
        }

        private async Task<RedirectResult> ShowHideMilestoneAsync(int id, bool isVisible)
        {
            using (var cn = Data.GetConnection())
            {
                var muv = await GetMilestoneUserViewAsync(cn, id);
                muv.IsVisible = isVisible;
                await cn.SaveAsync(muv, CurrentUser);
                return Redirect("/Dashboard/MyItems");
            }
        }

        /// <summary>
        /// Regular Postulate FindWhere  won't work with properties where value == 0, so I needed a dedicated query        
        /// </summary>
        private async Task<MilestoneUserView> GetMilestoneUserViewAsync(IDbConnection connection, int milestoneId)
        {
            return await connection.QuerySingleOrDefaultAsync<MilestoneUserView>(
                @"SELECT * FROM [dbo].[MilestoneUserView] WHERE [UserId]=@userId AND [MilestoneId]=@milestoneId",
                new { UserId, milestoneId }) ??
                new MilestoneUserView() { MilestoneId = milestoneId, UserId = UserId };
        }
    }
}