using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Services;
using Dapper.QX;
using Dapper.QX.Attributes;
//using Postulate.Base.Classes;
//using Dapper.QX.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using Dapper.QX.Models;
using Dapper.QX.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public enum EventLogsResultOrderBy
    {
        DateDesc,
        DateAsc
    }

	public class EventLogsResult : IWorkItemNumber, IWorkItemTitle
	{
		public string EventName { get; set; }
		public int Id { get; set; }
		public int EventId { get; set; }
		public int OrganizationId { get; set; }
		public int ApplicationId { get; set; }
		public int WorkItemId { get; set; }
		public string IconClass { get; set; }
		public string IconColor { get; set; }
		public string HtmlBody { get; set; }
		public string TextBody { get; set; }
		public string CreatedBy { get; set; }
		public string DisplayName { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime EventDate { get; set; }
		public int Number { get; set; }
		public int EstimateHours { get; set; }
		public decimal ColorGradientPosition { get; set; }
		public int ProjectId { get; set; }
		public string DisplayProjectName { get; set; }
		public string Title { get; set; }
		public int? ProjectPriority { get; set; }
        public string TeamName { get; set; }
        public string ApplicationName { get; set; }
        public WorkItemTitleViewField TitleViewField { get; set; } = WorkItemTitleViewField.Application | WorkItemTitleViewField.Project;

        public bool IsEditable(string userName)
		{
			return false;
		}
	}

	public class EventLogs : Query<EventLogsResult>, ITestableQuery
	{
        private readonly List<QueryTrace> _traces;

        public EventLogs() : base(
			@"SELECT TOP (100)
				[ev].[Name] AS [EventName],
				[el].*,
				[wi].[Number],
                [t].[Name] AS [TeamName],
                [wi].[ApplicationId],
                [app].[Name] AS [ApplicationName],
				COALESCE([wi].[ProjectId], 0) AS [ProjectId],
				COALESCE([p].[Nickname], [p].[Name]) AS [DisplayProjectName], [p].[Priority] AS [ProjectPriority],				
				[wi].[Title],
				COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0) AS [EstimateHours],
				COALESCE([gp].[ColorGradientPosition], 0) AS [ColorGradientPosition],
				CONVERT(date, [el].[DateCreated]) AS [EventDate],
				COALESCE([ou].[DisplayName], [el].[CreatedBy]) AS [DisplayName]
			FROM
				[dbo].[EventLog] [el]
				INNER JOIN [app].[Event] [ev] ON [el].[EventId]=[ev].[Id]
				INNER JOIN [dbo].[WorkItem] [wi] ON [el].[WorkItemId]=[wi].[Id]
                INNER JOIN [dbo].[Team] [t] ON [wi].[TeamId]=[t].[Id]
				LEFT JOIN [dbo].[Project] [p] ON [wi].[ProjectId]=[p].[Id]
                LEFT JOIN [dbo].[Application] [app] ON [wi].[ApplicationId]=[app].[Id]
				LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
				LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
				LEFT JOIN [dbo].[FnColorGradientPositions](@orgId) [gp] ON
					COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0) >= [gp].[MinHours] AND
					COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0) < [gp].[MaxHours]
				INNER JOIN [dbo].[AspNetUsers] [u] ON [el].[CreatedBy]=[u].[UserName]
				INNER JOIN [dbo].[OrganizationUser] [ou] ON 
					[u].[UserId]=[ou].[UserId] AND
					[wi].[OrganizationId]=[ou].[OrganizationId]
                {join}
			WHERE
				[el].[OrganizationId]=@orgId AND
				[el].[TeamId]=@teamId 
                {andWhere}				
			ORDER BY
				{orderBy}")
		{
		}

        public EventLogs(List<QueryTrace> traces = null) : this()
        {
            _traces = traces;
        }

		protected override void OnQueryExecuted(Dapper.QX.Models.QueryTrace queryTrace)
		{
			_traces?.Add(queryTrace);
		}

        [OrderBy(EventLogsResultOrderBy.DateDesc, "[el].[DateCreated] DESC")]
        [OrderBy(EventLogsResultOrderBy.DateAsc, "[el].[DateCreated] ASC")]
        public EventLogsResultOrderBy OrderBy { get; set; } = EventLogsResultOrderBy.DateDesc;

        public int OrgId { get; set; }

        public int TeamId { get; set; }

        [Join("INNER JOIN [dbo].[EventSubscription] [es] ON [el].[EventId]=[es].[EventId] AND [el].[ApplicationId]=[es].[ApplicationId] AND [es].[OrganizationId]=@orgId AND [es].[UserId]=@eventsUserId AND [es].[Visible]=1")]
        public bool MyEvents { get; set; }

		[Parameter]
        public int? EventsUserId { get; set; }

        [Where("[el].[ApplicationId]=@appId")]
		public int? AppId { get; set; }

		[Where("[el].[EventId] IN @eventIds")]
		public int[] EventIds { get; set; }

        [Where("[el].[EventId]=@eventId")]
        public int? EventId { get; set; }

        [Where("[el].[EventId] NOT IN @excludeEventIds")]
        public int[] ExcludeEventIds { get; set; }

        [Where("[ou].[UserId]=@userId")]
        public int? UserId { get; set; }

        [Where("[wi].[CloseReasonId]=@closeReasonId")]
        public int? CloseReasonId { get; set; }

        [Where("[el].[WorkItemId]=@workItemId")]
        public int? WorkItemId { get; set; }

		public IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new EventLogs() { OrgId = 0 };
			yield return new EventLogs() { AppId = 0 };
			yield return new EventLogs() { EventIds = new int[] { 1, 2, 3 } };
            yield return new EventLogs() { MyEvents = true, EventsUserId = 1 };
            yield return new EventLogs() { ExcludeEventIds = new int[] { 1, 2, 3 } };
		}

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}
	}
}