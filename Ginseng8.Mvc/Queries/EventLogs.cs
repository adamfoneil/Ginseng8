using Ginseng.Mvc.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
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
		public string ProjectName { get; set; }
		public string Title { get; set; }
		public int? ProjectPriority { get; set; }

		public bool IsEditable(string userName)
		{
			return false;
		}
	}

	public class EventLogs : Query<EventLogsResult>, ITestableQuery
	{
		public EventLogs() : base(
			@"SELECT TOP (100)
				[ev].[Name] AS [EventName],
				[el].*,
				[wi].[Number],
				COALESCE([wi].[ProjectId], 0) AS [ProjectId],
				[p].[Name] AS [ProjectName], [p].[Priority] AS [ProjectPriority],				
				[wi].[Title],
				COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0) AS [EstimateHours],
				COALESCE([gp].[ColorGradientPosition], 0) AS [ColorGradientPosition],
				CONVERT(date, [el].[DateCreated]) AS [EventDate],
				COALESCE([ou].[DisplayName], [el].[CreatedBy]) AS [DisplayName]
			FROM
				[dbo].[EventLog] [el]
				INNER JOIN [app].[Event] [ev] ON [el].[EventId]=[ev].[Id]
				INNER JOIN [dbo].[WorkItem] [wi] ON [el].[WorkItemId]=[wi].[Id]
				LEFT JOIN [dbo].[Project] [p] ON [wi].[ProjectId]=[p].[Id]
				LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
				LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
				LEFT JOIN [dbo].[FnColorGradientPositions](@orgId) [gp] ON
					COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0) >= [gp].[MinHours] AND
					COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0) < [gp].[MaxHours]
				INNER JOIN [dbo].[AspNetUsers] [u] ON [el].[CreatedBy]=[u].[UserName]
				INNER JOIN [dbo].[OrganizationUser] [ou] ON 
					[u].[UserId]=[ou].[UserId] AND
					[wi].[OrganizationId]=[ou].[OrganizationId]
			WHERE
				[el].[OrganizationId]=@orgId AND
				[el].[ApplicationId]=@appId
				{andWhere}
			ORDER BY
				[el].[DateCreated] DESC")
		{
		}

		public int OrgId { get; set; }

		public int AppId { get; set; }

		[Where("[el].[EventId] IN @eventIds")]
		public int[] EventIds { get; set; }

        [Where("[ou].[UserId]=@userId")]
        public int? UserId { get; set; }

		public IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new EventLogs() { OrgId = 0 };
			yield return new EventLogs() { AppId = 0 };
			yield return new EventLogs() { EventIds = new int[] { 1, 2, 3 } };
		}

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}
	}
}