using System.Collections.Generic;
using System.Data;
using Postulate.Base;
using Postulate.Base.Interfaces;

namespace Ginseng.Models.Queries
{
	public class InsertEventSubscriptionTextNotifications : Query<int>, ITestableQuery
	{
		public InsertEventSubscriptionTextNotifications() : base(
			@"INSERT INTO [dbo].[Notification] (
				[DateCreated], [Method], [SendTo], [Content], [SourceId], [SourceTable]
			) SELECT
				getutcdate(), 2, [u].[PhoneNumber], [el].[TextBody], [es].[Id], 'EventSubscription'
			FROM
				[dbo].[EventSubscription] [es]
				INNER JOIN [dbo].[EventLog] [el] ON
					[es].[EventId]=[el].[EventId] AND
					[es].[ApplicationId]=[el].[ApplicationId]
				INNER JOIN [dbo].[AspNetUsers] [u] ON [es].[UserId]=[u].[UserId]
			WHERE
				[el].[Id]=@id AND
				[es].[SendText]=1 AND
				[u].[PhoneNumber] IS NOT NULL AND
				[u].[PhoneNumberConfirmed]=1")
		{
		}

		public int Id { get; set; }

		public static IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new InsertEventSubscriptionTextNotifications() { Id = 0 };
		}

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}
	}
}