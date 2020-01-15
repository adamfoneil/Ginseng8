using System.Collections.Generic;
using System.Data;
using Ginseng.Models;
using Dapper.QX;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class MyActivityOrder : Query<Activity>, ITestableQuery
    {
        public MyActivityOrder() : base(
            @"SELECT [a].*, COALESCE([uao].[Value], [a].[Order]) AS [UserOrder]
            FROM [dbo].[Activity] [a]
            LEFT JOIN [dbo].[UserActivityOrder] [uao] ON [a].[Id]=[uao].[ActivityId] AND [uao].[UserId]=@userId
            WHERE [a].[OrganizationId]=@orgId AND [a].[IsActive]=1
            ORDER BY COALESCE([uao].[Value], [a].[Order])")
        {
        }

        public int OrgId { get; set; }
        public int UserId { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new MyActivityOrder() { UserId = 1 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}