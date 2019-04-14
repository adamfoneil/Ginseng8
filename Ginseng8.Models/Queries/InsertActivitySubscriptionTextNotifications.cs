using System.Collections.Generic;
using System.Data;
using Postulate.Base;
using Postulate.Base.Interfaces;

namespace Ginseng.Models.Queries
{
	public class InsertActivitySubscriptionTextNotifications : Query<int>, ITestableQuery
	{
		public InsertActivitySubscriptionTextNotifications() : base(
			@"INSERT INTO [dbo].[Notification] (
				[DateCreated], [Method], [SendTo], [Content], [SourceId], [SourceTable]
			) SELECT
				getutcdate(), 2, [u].[PhoneNumber], [el].[TextBody], [as].[Id], 'ActivitySubscription'
			FROM
				[dbo].[EventLog] [el]
				INNER JOIN [dbo].[HandOff] [ho] ON [el].[SourceId]=[ho].[Id]
				INNER JOIN [dbo].[WorkItem] [wi] ON [ho].[WorkItemId]=[wi].[Id]
				INNER JOIN [dbo].[ActivitySubscription] [as] ON
					[ho].[ToActivityId]=[as].[ActivityId] AND
					[wi].[ApplicationId]=[as].[ApplicationId]
				INNER JOIN [dbo].[AspNetUsers] [u] ON [as].[UserId]=[u].[UserId]
			WHERE
				[el].[SourceTable]='HandOff' AND
				[el].[Id]=@id AND
				[as].[SendText]=1 AND
				[u].[PhoneNumberConfirmed]=1 AND
				[u].[PhoneNumber] IS NOT NULL")
		{
		}

		public int Id { get; set; }

		public static IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new InsertActivitySubscriptionTextNotifications() { Id = 0 };
		}

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}
	}
}