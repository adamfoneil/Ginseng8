using Postulate.Base;
using Postulate.Base.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Models.Queries
{
    public class InsertEventSubscriptionAppNotifications : Query<int>, ITestableQuery
    {
        public InsertEventSubscriptionAppNotifications() : base(
            @"INSERT INTO [dbo].[Notification] (
				[EventLogId], [DateCreated], [Method], [SendTo], [Content], [SourceId], [SourceTable]
			) SELECT
				@id, getutcdate(), 3, [u].[UserName], [el].[HtmlBody], [es].[Id], 'EventSubscription'
			FROM
				[dbo].[EventSubscription] [es]
				INNER JOIN [dbo].[EventLog] [el] ON
					[es].[EventId]=[el].[EventId] AND
					[es].[ApplicationId]=[el].[ApplicationId]
				INNER JOIN [dbo].[AspNetUsers] [u] ON [es].[UserId]=[u].[UserId]
			WHERE
				[el].[Id]=@id AND
				[es].[InApp]=1")
        {
        }

        public int Id { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new InsertEventSubscriptionAppNotifications() { Id = 0 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}
