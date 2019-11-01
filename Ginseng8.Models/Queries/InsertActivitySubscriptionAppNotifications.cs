using Postulate.Base;
using Postulate.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Models.Queries
{
    public class InsertActivitySubscriptionAppNotifications : Query<int>, ITestableQuery
    {
        public InsertActivitySubscriptionAppNotifications() : base(
            @"INSERT INTO [dbo].[Notification] (
				[EventLogId], [DateCreated], [Method], [SendTo], [Content], [SourceId], [SourceTable]
			) SELECT
				@id, getutcdate(), 3, [u].[UserName], [el].[HtmlBody], [as].[Id], 'ActivitySubscription'
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
				[as].[InApp]=1")
        {
        }

        public int Id { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new InsertActivitySubscriptionAppNotifications() { Id = 0 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}
