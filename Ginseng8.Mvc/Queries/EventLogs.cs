using Postulate.Base;
using Postulate.Base.Attributes;
using System;

namespace Ginseng.Mvc.Queries
{
	public class EventLogsResult
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
		public DateTime DateCreated { get; set; }
		public int WorkItemNumber { get; set; }
		public DateTime EventDate { get; set; }
	}

	public class EventLogs : Query<EventLogsResult>
	{
		public EventLogs() : base(
			@"SELECT TOP (100)
				[ev].[Name] AS [EventName],
				[el].*,
				[wi].[Number] AS [WorkItemNumber],
				CONVERT(date, [el].[DateCreated]) AS [EventDate]
			FROM
				[dbo].[EventLog] [el]
				INNER JOIN [app].[Event] [ev] ON [el].[EventId]=[ev].[Id]
				INNER JOIN [dbo].[WorkItem] [wi] ON [el].[WorkItemId]=[wi].[Id]
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
	}
}