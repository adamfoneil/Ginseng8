using Postulate.Base;
using Postulate.Base.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Models.Queries
{
    public class InsertLabelSubscriptionAppNotifications : Query<int>, ITestableQuery
    {
        public InsertLabelSubscriptionAppNotifications() : base(
            @"INSERT INTO [dbo].[Notification] (
				[EventLogId], [DateCreated], [Method], [SendTo], [Content], [SourceId], [SourceTable]
			) SELECT
	            @id, getutcdate(), 3, [u].[UserName], [el].[HtmlBody], [ls].[Id], 'LabelSubscription'
            FROM
	            [dbo].[EventLog] [el]
                INNER JOIN [dbo].[WorkItemLabel] [wil] ON [el].[SourceId]=[wil].[Id]
                INNER JOIN [dbo].[LabelSubscription] [ls] ON
                    [wil].[LabelId]=[ls].[LabelId] AND
		            [el].[OrganizationId]=[ls].[OrganizationId]
                INNER JOIN [dbo].[AspNetUsers] [u] ON [ls].[UserId]=[u].[UserId]
            WHERE
	            [wil].[WorkItemId]=[el].[WorkItemId] AND
	            [el].[SourceTable]='WorkItemLabel' AND
	            [el].[Id]=@id AND
	            [ls].[InApp]=1 AND
                ([ls].[ApplicationId]=0 OR [ls].[ApplicationId]=[el].[ApplicationId])")
        {
        }

        public int Id { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new InsertLabelSubscriptionAppNotifications() { Id = 0 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}
