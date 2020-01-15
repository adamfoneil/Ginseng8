using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
	public class PendingNotificationResult
	{
		public int Id { get; set; }
		public int EventLogId { get; set; }
		public DateTime DateCreated { get; set; }
		public int Method { get; set; }
		public string SendTo { get; set; }
		public string Content { get; set; }
		public int SourceId { get; set; }
		public string SourceTable { get; set; }
		public DateTime? DateDelivered { get; set; }
		public string IconClass { get; set; }
		public string IconColor { get; set; }
		public int WorkItemNumber { get; set; }
		public string WorkItemTitle { get; set; }
		public string OrganizationName { get; set; }
        public string TeamName { get; set; }
		public string ApplicationName { get; set; }        
		public string EventName { get; set; }

        public string ParentName { get { return ApplicationName ?? TeamName; } }

        public async Task MarkDeliveredAsync(IDbConnection connection)
		{
			await connection.ExecuteAsync("UPDATE [dbo].[Notification] SET [DateDelivered]=getutcdate() WHERE [Id]=@id", new { Id });
		}

        public DateTime GetLocalDateCreated(int offset, bool adjustDst)
        {
            return UserProfile.GetLocalTime(DateCreated, offset, adjustDst);
        }
	}

	public class PendingNotifications : Query<PendingNotificationResult>, ITestableQuery
	{
		private static string BaseSql(int count = 10)
		{
			return $@"SELECT TOP ({count})
				[n].*, 
				[el].[IconClass], [el].[IconColor],
				[wi].[Number] AS [WorkItemNumber],
				[wi].[Title] AS [WorkItemTitle], 
				[org].[Name] AS [OrganizationName],
                [t].[Name] AS [TeamName],
				[app].[Name] AS [ApplicationName],
				[e].[Name] AS [EventName]
			FROM 
				[dbo].[Notification] [n]
				LEFT JOIN [dbo].[EventLog] [el] ON [n].[EventLogId]=[el].[Id]
				LEFT JOIN [app].[Event] [e] ON [el].[EventId]=[e].[Id]
				INNER JOIN [dbo].[WorkItem] [wi] ON [el].[WorkItemId]=[wi].[Id]
                INNER JOIN [dbo].[Team] [t] ON [el].[TeamId]=[t].[Id]
				LEFT JOIN [dbo].[Application] [app] ON [el].[ApplicationId]=[app].[Id]
				INNER JOIN [dbo].[Organization] [org] ON [el].[OrganizationId]=[org].[Id]
			WHERE
				[DateDelivered] IS NULL {{andWhere}} 
			ORDER BY 
				[n].[DateCreated] ASC";
		}

		public PendingNotifications() : base(BaseSql(10))
		{
		}

		public PendingNotifications(int count) : base(BaseSql(count))
		{
		}

		[Where("[Method]=@method")]
		public DeliveryMethod? Method { get; set; }

        [Where("[SendTo]=@sendTo")]
        public string SendTo { get; set; }

		public IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new PendingNotifications(10);
			yield return new PendingNotifications(10) { Method = DeliveryMethod.Email };
		}

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}
	}
}