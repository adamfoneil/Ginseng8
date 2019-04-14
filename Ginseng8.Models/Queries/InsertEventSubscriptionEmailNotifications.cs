using System.Collections.Generic;
using System.Data;
using Postulate.Base;
using Postulate.Base.Interfaces;

namespace Ginseng.Models.Queries
{
	public class InsertEventSubscriptionEmailNotifications : Query<int>, ITestableQuery
	{
		public InsertEventSubscriptionEmailNotifications() : base(
			@"INSERT INTO [dbo].[Notification] (
				[DateCreated], [Method], [SendTo], [Content], [SourceId], [SourceTable]
			) SELECT
				getutcdate(), 1, [u].[Email], [el].[HtmlBody], [es].[Id], 'EventSubscription'
			FROM
				[dbo].[EventSubscription] [es]
				INNER JOIN [dbo].[EventLog] [el] ON
					[es].[EventId]=[el].[EventId] AND
					[es].[ApplicationId]=[el].[ApplicationId]
				INNER JOIN [dbo].[AspNetUsers] [u] ON [es].[UserId]=[u].[UserId]
			WHERE
				[el].[Id]=@id AND
				[es].[SendEmail]=1 AND
				[u].[EmailConfirmed]=1")
		{
		}

		public int Id { get; set; }

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}

		public static IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new InsertEventSubscriptionEmailNotifications() { Id = 0 };
		}
	}
}