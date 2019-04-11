using Postulate.Base;
using Postulate.Base.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Models.Queries
{
	public class InsertActivitySubscriptionEmailNotifications : Query<int>, ITestableQuery
	{
		public InsertActivitySubscriptionEmailNotifications() : base(
			@"INSERT INTO [dbo].[Notification] (
				[DateCreated], [Method], [SendTo], [Content], [SourceId], [SourceTable]
			) SELECT
				getutcdate(), 1, [u].[Email], [el].[HtmlBody], [as].[Id], 'ActivitySubscription'
			FROM
				[dbo].[HandOff] [ho]
				INNER JOIN [dbo].[WorkItem] [wi] ON [ho].[WorkItemId]=[wi].[Id]
				INNER JOIN [dbo].[ActivitySubscription] [as] ON
					[ho].[ToActivityId]=[as].[ActivityId] AND
					[wi].[ApplicationId]=[as].[ApplicationId]				
				INNER JOIN [dbo].[AspNetUsers] [u] ON [as].[UserId]=[u].[UserId]
			WHERE
				[as].[ActivityId]=@activityId AND
				[as].[ApplicationId]=@appId AND
				[as].[SendEmail]=1 AND
				[u].[EmailConfirmed]=1")
		{
		}

		public int ActivityId { get; set; }
		public int AppId { get; set; }

		public static IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new InsertActivitySubscriptionEmailNotifications() { ActivityId = 0, AppId = 0 };
		}

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}
	}
}