using System.Collections.Generic;
using System.Data;
using Postulate.Base;
using Postulate.Base.Interfaces;

namespace Ginseng.Models.Queries
{
    public class InsertAssignedDevAppNotification : Query<int>, ITestableQuery
    {
        public InsertAssignedDevAppNotification() : base(
            @"INSERT INTO [dbo].[Notification] (
				[EventLogId], [DateCreated], [Method], [SendTo], [Content], [SourceId], [SourceTable]
			) SELECT
				@id, getutcdate(), 3, [u].[UserName], [el].[HtmlBody], [ou].[Id], 'OrganizationUser'
			FROM
				[dbo].[EventLog] [el]
				INNER JOIN [dbo].[WorkItem] [wi] ON [el].[WorkItemId]=[wi].[Id]
				INNER JOIN [dbo].[AspNetUsers] [u] ON [wi].[DeveloperUserId]=[u].[UserId]
                INNER JOIN [dbo].[OrganizationUser] [ou] ON
                    [ou].[OrganizationId]=[wi].[OrganizationId] AND
                    [ou].[UserId]=[wi].[DeveloperUserId]
			WHERE
				[el].[Id]=@id AND
				[ou].[InApp]=1")
        {
        }

        public int Id { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new InsertAssignedDevAppNotification() { Id = 0 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}
