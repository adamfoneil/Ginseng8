using System.Collections.Generic;
using System.Data;
using Dapper.QX;
using Dapper.QX.Attributes;
using Dapper.QX.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class ItemCountsByLabelResult
    {
        public int LabelId { get; set; }
        public int Count { get; set; }
    }

    public class ItemCountsByLabel : Query<ItemCountsByLabelResult>, ITestableQuery
    {
        public ItemCountsByLabel() : base(
            @"SELECT
                [wil].[LabelId],
                COUNT(1) AS [Count]
            FROM
                [dbo].[WorkItem] [wi]
                INNER JOIN [dbo].[WorkItemLabel] [wil] ON [wi].[Id]=[wil].[WorkItemId]
                LEFT JOIN [dbo].[Activity] [act] ON [wi].[ActivityId]=[act].[Id]
                LEFT JOIN [app].[Responsibility] [r] ON [act].[ResponsibilityId]=[r].[Id]
                LEFT JOIN [dbo].[WorkItemPriority] [pri] ON [wi].[Id]=[pri].[WorkItemId]
            WHERE
                [wi].[OrganizationId]=@orgId AND
                [wi].[TeamId]=@teamId AND
                [wi].[CloseReasonId] IS NULL {andWhere}
            GROUP BY
                [wil].[LabelId]")
        {
        }

        public int OrgId { get; set; }
        public int TeamId { get; set; }

        [Where("[wi].[ApplicationId]=@appId")]
        public int? AppId { get; set; }

        [Case(true, "[wi].[ProjectId] IS NOT NULL")]
        [Case(false, "[wi].[ProjectId] IS NULL")]
        public bool? HasProject { get; set; }

        [Case(false, OpenWorkItems.AssignedUserExpression + " IS NULL")]
        [Case(true, OpenWorkItems.AssignedUserExpression + " IS NOT NULL")]
        public bool? HasAssignedUserId { get; set; }

        [Case(true, "[pri].[Value] IS NOT NULL")]
        [Case(false, "[pri].[Value] IS NULL")]
        public bool? HasPriority { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new ItemCountsByLabel() { OrgId = 1, AppId = 1, HasProject = false };
            yield return new ItemCountsByLabel() { OrgId = 1, AppId = 1, HasProject = true };
            yield return new ItemCountsByLabel() { OrgId = 1, AppId = 1, HasAssignedUserId = true };
            yield return new ItemCountsByLabel() { OrgId = 1, AppId = 1, HasAssignedUserId = false };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}
