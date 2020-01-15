using System.Collections.Generic;
using System.Data;
using Dapper.QX;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class LabelSubscriptionUsersResult
    {
        public int LabelId { get; set; }
        public string UserName { get; set; }
    }

    public class LabelSubscriptionUsers : Query<LabelSubscriptionUsersResult>, ITestableQuery
    {
        public LabelSubscriptionUsers() : base(
            @"SELECT
                [ls].[LabelId],
                COALESCE([ou].[DisplayName], [u].[UserName]) AS [UserName]
            FROM
                [dbo].[LabelSubscription] [ls]
                INNER JOIN [dbo].[OrganizationUser] [ou] ON
                    [ls].[OrganizationId]=[ou].[OrganizationId] AND
                    [ls].[UserId]=[ou].[UserId]
                INNER JOIN [dbo].[AspNetUsers] [u] ON [ou].[UserId]=[u].[UserId]
            WHERE
                [ls].[OrganizationId]=@orgId AND
                [ls].[SendEmail]=1
            GROUP BY
                [ls].[LabelId],
                COALESCE([ou].[DisplayName], [u].[UserName])")
        {
        }

        public int OrgId { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new LabelSubscriptionUsers() { OrgId = 0 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}