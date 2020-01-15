using Dapper.QX;
using Dapper.QX.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
    public class InsertDefaultLabelSubscriptions : Query<int>, ITestableQuery
    {
        public InsertDefaultLabelSubscriptions() : base(
            @"INSERT INTO [dbo].[LabelSubscription] (
                [OrganizationId], [ApplicationId], [UserId], [LabelId], [SendEmail], [SendText], [InApp], [CreatedBy], [DateCreated]
            ) SELECT
                @orgId, @appId, @userId, [lbl].[Id], 0, 0, 0, @userName, getdate()
            FROM
                [dbo].[Label] [lbl]
            WHERE
                [lbl].[OrganizationId]=@orgId AND
                NOT EXISTS(SELECT 1 FROM [dbo].[LabelSubscription] WHERE [OrganizationId]=@orgId AND [ApplicationId]=@appId AND [UserId]=@userId AND [LabelId]=[lbl].[Id])")
        {
        }

        public int OrgId { get; set; }
        public int UserId { get; set; }
        public int AppId { get; set; }
        public string UserName { get; set; }        

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new InsertDefaultLabelSubscriptions() { OrgId = 0, UserId = 0, UserName = "system" };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}