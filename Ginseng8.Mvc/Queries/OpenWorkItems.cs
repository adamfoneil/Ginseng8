using Dapper.QX;
using Dapper.QX.Attributes;
using Dapper.QX.Models;
using Ginseng.Models;
using Ginseng.Models.Enums.Freshdesk;
using Ginseng.Models.Interfaces;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Services;
using Postulate.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
    /// <summary>
    /// see <see cref="PriorityGroup"/>
    /// </summary>
    public enum PriorityGroupOptions
    {
        WorkOnNext = 1,
        Backlog = 2,
        Assigned = 3,
        Closed = 4
    }

    public enum HungReason
    {
        None = 0,
        HasImpediment = 1, // technical issue
        IsPaused = 2, // has activity, but unassigned
        IsStopped = 3 // has milestone, but unassigned
    }

    public class OpenWorkItemsResult : IWorkItemNumber, IWorkItemTitle, IMilestoneHeader
    {
        public const string ImpedimentIcon = Comment.ImpedimentIcon;
        public const string ImpedimentColor = "darkred";
        public const string UnestimatedIcon = "fas fa-question-circle";
        public const string UnestimatedColor = "mediumpurple";
        public const string StoppedIcon = "fas fa-stop-circle";
        public const string StoppedColor = "orangered";
        public const string TicketIcon = "fas fa-ticket-alt";
        public const string TicketColor = "#1a7172";
        public const string PausedIcon = "fas fa-pause-circle";

        public const string ImpedimentModifier = "impediment";
        public const string UnestimatedModifier = "unestimated";

        public const string StoppedModifier = "stopped";
        public const string TicketModifier = "ticket";

        public int Id { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public int? Priority { get; set; }
        public string TextBody { get; set; }
        public string HtmlBody { get; set; }
        public int? BusinessUserId { get; set; }
        public int? DeveloperUserId { get; set; }
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public bool HasImpediment { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string DisplayProjectName { get; set; }
        public int? ProjectPriority { get; set; }
        public string PriorityTier { get; set; }
        public int? DataModelId { get; set; }
        public int MilestoneId { get; set; }
        public string MilestoneName { get; set; }
        public DateTime? MilestoneDate { get; set; }
        public DateTime SortMilestoneDate { get; set; }
        public int? MilestoneDaysAway { get; set; }
        public int? CloseReasonId { get; set; }
        public string CloseReasonName { get; set; }
        public int ActivityId { get; set; }
        public string ActivityName { get; set; }
        public int? ActivityOrder { get; set; }
        public string BusinessUserName { get; set; }
        public string DeveloperUserName { get; set; }
        public int? AssignedUserId { get; set; }
        public string AssignedUserName { get; set; }
        public string WorkItemSize { get; set; }
        public int? SizeId { get; set; }
        public int? DevEstimateHours { get; set; }
        public int? SizeEstimateHours { get; set; }
        public int EstimateHours { get; set; }
        public string WorkItemUserIdColumn { get; set; }
        public decimal ColorGradientPosition { get; set; }
        public int? LastHandOffId { get; set; }
        public string HandOffUserName { get; set; }
        public string HandOffBody { get; set; }
        public bool? IsForward { get; set; }
        public string FromActivityName { get; set; }
        public DateTime? HandOffDate { get; set; }
        public string CreatedByName { get; set; }
        public DateTime DateCreated { get; set; }
        public PriorityGroupOptions PriorityGroup { get; set; }
        public string CreatedBy { get; set; }
        public bool IsHelpdeskTicket { get; set; }
        public string FreshdeskUrl { get; set; }
        public long FDTicketId { get; set; }
        public string FDTicketSubject { get; set; }
        public long FDCompanyId { get; set; }
        public string FDCompanyName { get; set; }
        public long FDContactId { get; set; }
        public string FDContactName { get; set; }
        public TicketStatus FDTicketStatus { get; set; }
        public bool UseApplications { get; set; }
        public int? MyActivityOrder { get; set; }
        public bool IsMilestoneVisible { get { return true; } }

        /// <summary>
        /// Lets us use a single property to switch between the app or team Id based on whether the team uses applications
        /// </summary>
        public int ProjectParentId
        {
            get { return (UseApplications) ? ApplicationId : TeamId; }
        }

        public string ProjectParentField
        {
            get { return (UseApplications) ? "applicationId" : "teamId"; }
        }

        public ProjectParentType ProjectParentType
        {
            get { return (UseApplications) ? ProjectParentType.Application : ProjectParentType.Team; }
        }

        public string ProjectParentName
        {
            get { return (UseApplications) ? ApplicationName : TeamName; }
        }

        /// <summary>
        /// Used to create hidden fields for inserting work items.
        /// It varies whether we need an applicationId based on the kind of team
        /// </summary>        
        public Dictionary<string, int> ContextFields()
        {
            var result = new Dictionary<string, int>();
            result.Add("teamId", TeamId);
            if (UseApplications) result.Add("applicationId", ApplicationId);
            return result;
        }

        public bool IsHung
        {
            get { return HasImpediment || IsPaused() || IsStopped(); }
        }

        public HungReason HungReason
        {
            get
            {
                return
                    (HasImpediment) ? HungReason.IsPaused :
                    (IsPaused()) ? HungReason.IsPaused :
                    (IsStopped()) ? HungReason.IsStopped :
                    HungReason.None;
            }
        }

        public WorkItemTitleViewField TitleViewField { get; set; }

        public bool IsEditable(string userName)
        {
            return CreatedBy.Equals(userName);
        }

        public IEnumerable<Modifier> GetModifiers()
        {
            if (HasImpediment) yield return new Modifier() { Key = ImpedimentModifier, Icon = ImpedimentIcon, Color = ImpedimentColor, Description = "Something is impeding progress, described in comments" };
            if (EstimateHours == 0) yield return new Modifier() { Key = UnestimatedModifier, Icon = UnestimatedIcon, Color = UnestimatedColor, Description = "Item has no estimate" };
            if (IsStopped()) yield return new Modifier() { Key = StoppedModifier, Icon = StoppedIcon, Color = StoppedColor, Description = "Item is in a milestone, but has no activity" };
            if (IsHelpdeskTicket) yield return new Modifier() { Key = TicketModifier, Icon = TicketIcon, Color = TicketColor, Description = "Item was generated from a Freshdesk ticket" };
        }

        public string ActivityStatus()
        {
            string assignedTo = (AssignedUserId.HasValue) ? AssignedUserName : "paused";
            return $"{ActivityName ?? "(not started)"} - {assignedTo}";
        }

        public bool IsPaused()
        {
            return (ActivityId != 0 && !AssignedUserId.HasValue);
        }

        public bool IsStopped()
        {
            return (MilestoneId != 0 && ActivityId == 0);
        }

        public class Modifier
        {
            public string Key { get; set; }
            public string Icon { get; set; }
            public string Color { get; set; }
            public string Description { get; set; }
        }
    }

    public class OpenWorkItems : Query<OpenWorkItemsResult>, ITestableQuery
    {
        public const string AssignedUserExpression =
            "(CASE [act].[ResponsibilityId] WHEN 1 THEN [wi].[BusinessUserId] WHEN 2 THEN [wi].[DeveloperUserId] END)";

        private const string PriorityGroupExpression =
            "(CASE WHEN [wi].[CloseReasonId] IS NOT NULL THEN 4 WHEN [pri].[Id] IS NOT NULL AND " + AssignedUserExpression + " IS NULL AND [wi].[ActivityId] IS NULL THEN 1 WHEN [pri].[Id] IS NULL AND " + AssignedUserExpression + " IS NULL THEN 2 ELSE 3 END)";

        private readonly List<QueryTrace> _traces;

        public OpenWorkItems() : base(
            $@"SELECT
				[wi].[Id],
				[wi].[Number],
				[pri].[Value] AS [Priority],
				[wi].[Title],
				[wi].[TextBody], [wi].[HtmlBody],
				[wi].[BusinessUserId],
				[wi].[DeveloperUserId],
				COALESCE([wi].[ApplicationId], 0) AS [ApplicationId], [app].[Name] AS [ApplicationName],
                [wi].[TeamId], [t].[Name] AS [TeamName],
				[wi].[HasImpediment],
				COALESCE([createdBy_ou].[DisplayName], [wi].[CreatedBy]) AS [CreatedByName], [wi].[DateCreated],
				COALESCE([wi].[ProjectId], 0) AS [ProjectId], COALESCE([p].[Name], '(no project)') AS [ProjectName],
                COALESCE([p].[Nickname], [p].[Name]) AS [DisplayProjectName],
				[p].[Priority] AS [ProjectPriority],				
				COALESCE([wi].[MilestoneId], 0) AS [MilestoneId], COALESCE([ms].[Name], '(no milestone)') AS [MilestoneName], [ms].[Date] AS [MilestoneDate], COALESCE([ms].[Date], '12/31/9999') AS [SortMilestoneDate], DATEDIFF(d, getdate(), [ms].[Date]) AS [MilestoneDaysAway],
				[wi].[CloseReasonId], [cr].[Name] AS [CloseReasonName],
				COALESCE([wi].[ActivityId], 0) AS [ActivityId],
				[act].[Name] AS [ActivityName],
				[act].[Order] AS [ActivityOrder],
				COALESCE([biz_ou].[DisplayName], [ousr].[UserName]) AS [BusinessUserName],
				COALESCE([dev_ou].[DisplayName], [dusr].[UserName]) AS [DeveloperUserName],
				CASE [act].[ResponsibilityId]
					WHEN 1 THEN COALESCE([biz_ou].[DisplayName], [ousr].[UserName])
					WHEN 2 THEN COALESCE([dev_ou].[DisplayName], [dusr].[UserName])
				END [AssignedUserName],
				{AssignedUserExpression} AS [AssignedUserId],
				[p].[DataModelId],
				[sz].[Name] AS [WorkItemSize],
				[wi].[SizeId],
				[wid].[EstimateHours] AS [DevEstimateHours],
				[sz].[EstimateHours] AS [SizeEstimateHours],
				COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0) AS [EstimateHours],
				[r].[WorkItemUserIdColumn],
				COALESCE([gp].[ColorGradientPosition], 0) AS [ColorGradientPosition],
				COALESCE([ho_ou].[DisplayName], [housr].[UserName]) AS [HandOffUserName],
				[wi].[LastHandOffId],
				[ho].[IsForward],
				[from_act].[Name] AS [FromActivityName],
				[ho].[HtmlBody] AS [HandOffBody],
				[ho].[DateCreated] AS [HandOffDate],
				{PriorityGroupExpression} AS [PriorityGroup],
				[wi].[CreatedBy],
                CONVERT(bit, CASE
                    WHEN [wit].[Id] IS NOT NULL THEN 1
                    ELSE 0
                END) AS [IsHelpdeskTicket],
                [wit].[TicketId] AS [FDTicketId],
                [wit].[CompanyId] AS [FDCompanyId],
                [wit].[CompanyName] AS [FDCompanyName],
                [wit].[ContactId] AS [FDContactId],
                [wit].[ContactName] AS [FDContactName],
                [wit].[Subject] AS [FDTicketSubject],
                [wit].[TicketStatus] AS [FDTicketStatus],
                [org].[FreshdeskUrl],
                [t].[UseApplications],
                [uao].[Value] AS [MyActivityOrder]
			FROM
				[dbo].[WorkItem] [wi]
                INNER JOIN [dbo].[Organization] [org] ON [wi].[OrganizationId]=[org].[Id]
                INNER JOIN [dbo].[Team] [t] ON [wi].[TeamId]=[t].[Id]				                
                LEFT JOIN [dbo].[Application] [app] ON [wi].[ApplicationId]=[app].[Id]                
				LEFT JOIN [dbo].[WorkItemPriority] [pri] ON [wi].[Id]=[pri].[WorkItemId]
				LEFT JOIN [dbo].[Project] [p] ON [wi].[ProjectId]=[p].[Id]
				LEFT JOIN [dbo].[Activity] [act] ON [wi].[ActivityId]=[act].[Id]
				LEFT JOIN [app].[Responsibility] [r] ON [act].[ResponsibilityId]=[r].[Id]
				LEFT JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
				LEFT JOIN [app].[CloseReason] [cr] ON [wi].[CloseReasonId]=[cr].[Id]
				LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
				LEFT JOIN [dbo].[HandOff] [ho] ON [wi].[LastHandOffId]=[ho].[Id]
				LEFT JOIN [dbo].[OrganizationUser] [biz_ou] ON
					[wi].[OrganizationId]=[biz_ou].[OrganizationId] AND
					[wi].[BusinessUserId]=[biz_ou].[UserId]
				LEFT JOIN [dbo].[AspNetUsers] [ousr] ON [wi].[BusinessUserId]=[ousr].[UserId]
				LEFT JOIN [dbo].[OrganizationUser] [dev_ou] ON
					[wi].[OrganizationId]=[dev_ou].[OrganizationId] AND
					[wi].[DeveloperUserId]=[dev_ou].[UserId]
				LEFT JOIN [dbo].[AspNetUsers] [housr] ON [ho].[FromUserId]=[housr].[UserId]
				LEFT JOIN [dbo].[OrganizationUser] [ho_ou] ON
					[wi].[OrganizationId]=[ho_ou].[OrganizationId] AND
					[ho].[FromUserId]=[ho_ou].[UserId]
				LEFT JOIN [dbo].[Activity] [from_act] ON [ho].[FromActivityId]=[from_act].[Id]
				LEFT JOIN [dbo].[AspNetUsers] [dusr] ON [wi].[DeveloperUserId]=[dusr].[UserId]
				LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
				LEFT JOIN [dbo].[AspNetUsers] [createdBy_user] ON [wi].[CreatedBy]=[createdBy_user].[UserName]
				LEFT JOIN [dbo].[OrganizationUser] [createdBy_ou] ON
					[wi].[OrganizationId]=[createdBy_ou].[OrganizationId] AND
					[createdBy_user].[UserId]=[createdBy_ou].[UserId]
				LEFT JOIN [dbo].[FnColorGradientPositions](@orgId) [gp] ON
					COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0) >= [gp].[MinHours] AND
					COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0) < [gp].[MaxHours]
                LEFT JOIN [dbo].[WorkItemTicket] [wit] ON 
                    [wit].[OrganizationId]=[wi].[OrganizationId] AND
                    [wit].[WorkItemNumber]=[wi].[Number]   
                LEFT JOIN [dbo].[UserActivityOrder] [uao] ON [wi].[ActivityId]=[uao].[ActivityId] AND [uao].[UserId]=@activityUserId
				{{join}}
            WHERE
				[wi].[OrganizationId]=@orgId {{andWhere}}
            ORDER BY
				COALESCE([pri].[Value], 100000),
				[wi].[Number]
			{{offset}}")
        {
        }

        public OpenWorkItems(List<QueryTrace> traces = null) : this()
        {
            _traces = traces;
        }

        protected override void OnQueryExecuted(Dapper.QX.Models.QueryTrace queryTrace)
        {
            _traces?.Add(queryTrace);
        }

        public int OrgId { get; set; }

        [Where("[wi].[Id]=@id")]
        public int? Id { get; set; }

        [Where("[wi].[TeamId]=@teamId")]
        public int? TeamId { get; set; }

        [Offset(30)]
        public int? Page { get; set; }

        [Join("INNER JOIN [dbo].[ActivitySubscription] [asub] ON [wi].[ActivityId]=[asub].[ActivityId] AND [wi].[ApplicationId]=[asub].[ApplicationId] AND [asub].[UserId]=@activityUserId")]
        public bool InMyActivities { get; set; }

        [Join("INNER JOIN [dbo].[FnAppNotifyWorkItems](@orgId, @notifyUserName) [notify] ON [wi].[Id]=[notify].[WorkItemId]")]
        public bool InMyNotifications { get; set; }

        /// <summary>
        /// Use this when InMyActivities = true or WithMyActivityOrder = true
        /// </summary>		
        public int ActivityUserId { get; set; }

        [Join("LEFT JOIN [dbo].[FnWorkItemSchedule](@orgId, @scheduleUserId) [wis] ON [wi].[Number]=[wis].[Number]")]
        public bool WithWorkSchedule { get; set; }

        /// <summary>
        /// Use with InMyNotifications
        /// </summary>
        public string NotifyUserName { get; set; }

        /// <summary>
        /// Use this when WithWorkSchedule = true
        /// </summary>
        public int ScheduleUserId { get; set; }

        [Where("[wis].[Date]=@workDate")]
        public DateTime? WorkDate { get; set; }

        [Case(true, "[wi].[CloseReasonId] IS NULL")]
        [Case(false, "[wi].[CloseReasonId] IS NOT NULL")]
        public bool? IsOpen { get; set; } = true;

        [Where("[wi].[Number]=@number")]
        public int? Number { get; set; }

        [Case(0, "[wi].[CloseReasonId] IS NULL")]
        [Case(-1, "[wi].[CloseReasonId] IS NOT NULL")]
        [Where("[wi].[CloseReasonId]=@closeReasonId")]
        public int? CloseReasonId { get; set; }

        [Where(PriorityGroupExpression + "=@priorityGroupId")]
        public int? PriorityGroupId { get; set; }

        [Case(0, AssignedUserExpression + " IS NULL")]
        [Where(AssignedUserExpression + "=@assignedUserId")]
        public int? AssignedUserId { get; set; }

        [Where("[wi].[DeveloperUserId]=@developerUserId")]
        public int? DeveloperUserId { get; set; }

        [Where("[wi].[ApplicationId]=@appId")]
        public int? AppId { get; set; }

        [Case(true, "[wi].[MilestoneId] IS NOT NULL")]
        [Case(false, "[wi].[MilestoneId] IS NULL")]
        public bool? HasMilestone { get; set; }

        [Case(false, "([wi].[MilestoneId] IS NULL OR [ms].[Date]<getdate())")]
        [Case(true, "[ms].[Date]>getdate()")]
        public bool? HasFutureMilestone { get; set; }

        [Case(false, "[wi].[ProjectId] IS NULL")]
        [Case(true, "[wi].[ProjectId] IS NOT NULL")]
        public bool? HasProject { get; set; }

        [Where("([app].[AllowNewItems]=@dataEntryApps AND [app].[IsActive]=1)")]
        public bool? DataEntryApps { get; set; }

        [Case(true, "[pri].[Value] IS NOT NULL")]
        [Case(false, "[pri].[Value] IS NULL")]
        public bool? HasPriority { get; set; }

        [Where("EXISTS(SELECT 1 FROM [dbo].[WorkItemLabel] WHERE [WorkItemId]=[wi].[Id] AND [LabelId] IN @labelIds)")]
        public int[] LabelIds { get; set; }

        [Case(0, "[wi].[ProjectId] IS NULL")]
        [Where("[wi].[ProjectId]=@projectId")]
        public int? ProjectId { get; set; }

        [Where("EXISTS(SELECT 1 FROM [dbo].[WorkItemLabel] WHERE [WorkItemId]=[wi].[Id] AND [LabelId]=@labelId)")]
        public int? LabelId { get; set; }

        [Case(0, "[wi].[MilestoneId] IS NULL")]
        [Where("[wi].[MilestoneId]=@milestoneId")]
        public int? MilestoneId { get; set; }

        [Case(0, "[wi].[SizeId] IS NULL")]
        [Where("[wi].[SizeId]=@sizeId")]
        public int? SizeId { get; set; }

        [Case(0, "[wi].[ActivityId] IS NULL")]
        [Where("[wi].[ActivityId]=@activityId")]
        public int? ActivityId { get; set; }

        [Where("[wi].[ActivityId] IN @activityIds")]
        public int[] ActivityIds { get; set; }

        [Case(false, AssignedUserExpression + " IS NULL")]
        [Case(true, AssignedUserExpression + " IS NOT NULL")]
        public bool? HasAssignedUserId { get; set; }

        [Case(true, "[wi].[ActivityId] IS NOT NULL AND (" + AssignedUserExpression + ") IS NULL")]
        public bool? IsPaused { get; set; }

        [Case(true, "[wi].[MilestoneId] IS NOT NULL AND [wi].[ActivityId] IS NULL")]
        public bool? IsStopped { get; set; }

        [Phrase("wi].[Title", "wi].[TextBody")]
        public string TitleAndBodySearch { get; set; }

        [Where("[ms].[Date]<getdate()")]
        public bool? IsPastDue { get; set; }

        [Case(true, "[wit].[Id] IS NOT NULL")]
        public bool? IsFreshdeskTicket { get; set; }

        [Where("YEAR([ms].[Date])=@milestoneYear")]
        public int? MilestoneYear { get; set; }

        [Where("MONTH([ms].[Date])=@milestoneMonth")]
        public int? MilestoneMonth { get; set; }

        [Case(true, "EXISTS(SELECT 1 FROM [dbo].[PinnedWorkItem] WHERE [WorkItemId]=[wi].[Id] AND [UserId]=@myUserId)")]
        [Case(false, "NOT EXISTS(SELECT 1 FROM [dbo].[PinnedWorkItem] WHERE [WorkItemId]=[wi].[Id] AND [UserId]=@myUserId)")]
        public bool? MyPinnedItems { get; set; }

        [Parameter]
        public int? MyUserId { get; set; }

        [Join("INNER JOIN [dbo].[MilestoneUserView] [muv] ON COALESCE([wi].[MilestoneId], 0)=[muv].[MilestoneId]")]
        public bool VisibleMilestones { get; set; }

        /// <summary>
        /// Use this when VisibleMilestones == true
        /// </summary>
        [Where("([muv].[UserId]=@visibleToUserId AND [muv].[IsVisible]=1)")]
        public int? VisibleToUserId { get; set; }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            // ideally here, you return instances of your query with each parameter set to get coverage of all the generated SQL combinations.
            // we don't care what data is returned, only that the SQL compiles

            yield return new OpenWorkItems() { OrgId = 0 };
            yield return new OpenWorkItems() { IsOpen = true };
            yield return new OpenWorkItems() { Number = 0 };
            yield return new OpenWorkItems() { AssignedUserId = 0 };
            yield return new OpenWorkItems() { AppId = 0 };
            yield return new OpenWorkItems() { HasMilestone = true };
            yield return new OpenWorkItems() { HasMilestone = false };
            yield return new OpenWorkItems() { ProjectId = 0 };
            yield return new OpenWorkItems() { LabelId = 1 };
            yield return new OpenWorkItems() { MilestoneId = 1 };
            yield return new OpenWorkItems() { ActivityId = 1 };
            yield return new OpenWorkItems() { ActivityIds = new int[] { 1, 2, 3, } };
            yield return new OpenWorkItems() { HasAssignedUserId = true };
            yield return new OpenWorkItems() { HasAssignedUserId = false };
            yield return new OpenWorkItems() { IsPaused = true };
            yield return new OpenWorkItems() { IsStopped = true };
            yield return new OpenWorkItems() { TitleAndBodySearch = "whatever this that" };
            yield return new OpenWorkItems() { IsPastDue = true };
            yield return new OpenWorkItems() { InMyActivities = true, ActivityUserId = 0 };
            yield return new OpenWorkItems() { IsFreshdeskTicket = true };
            yield return new OpenWorkItems() { HasFutureMilestone = false };
            yield return new OpenWorkItems() { HasFutureMilestone = true };
            yield return new OpenWorkItems() { LabelIds = new int[] { 1, 2, 3 } };
        }
    }
}