using System.Collections.Generic;
using System.Data;
using Postulate.Base;
using Postulate.Base.Interfaces;

namespace Ginseng.Models.Queries
{
    public class InsertAssignmentBizAppNotification : Query<int>, ITestableQuery
    {
        public InsertAssignmentBizAppNotification() : base(
            @"INSERT INTO [dbo].[Notification] (
				[EventLogId], [DateCreated], [Method], [SendTo], [Content], [SourceId], [SourceTable]
			) SELECT
				@id, getutcdate(), 3, [u].[UserName], [el].[HtmlBody], [ou].[Id], 'OrganizationUser'
			FROM
				[dbo].[EventLog] [el]
				INNER JOIN [dbo].[WorkItem] [wi] ON [el].[WorkItemId]=[wi].[Id]
				INNER JOIN [dbo].[AspNetUsers] [u] ON [wi].[BusinessUserId]=[u].[UserId]
                INNER JOIN [dbo].[OrganizationUser] [ou] ON
                    [ou].[OrganizationId]=[wi].[OrganizationId] AND
                    [ou].[UserId]=[wi].[BusinessUserId]
			WHERE
				[el].[Id]=@id AND
				[ou].[InApp]=1")
        {
        }

        public int Id { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new InsertAssignmentBizAppNotification() { Id = 0 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}
