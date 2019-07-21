using System.Collections.Generic;
using System.Data;
using Postulate.Base;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class WorkItemEstimateHoursResult
    {
        public int Id { get; set; }        
        public int Hours { get; set; }
    }

    public class WorkItemEstimateHours : Query<WorkItemEstimateHoursResult>, ITestableQuery
    {
        public WorkItemEstimateHours() : base(
            @"SELECT
                [wi].[Id],
                COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0) AS [Hours]
            FROM
                [dbo].[WorkItem] [wi]
                LEFT JOIN [dbo].[WorkItemPriority] [wip] ON [wi].[Id]=[wip].[WorkItemId]
                LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
                LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
            WHERE
                [wi].[CloseReasonId] IS NULL AND
                [wi].[OrganizationId]=@orgId AND
                [wi].[DeveloperUserId]=@userId AND
                [wi].[MilestoneId]=@milestoneId
            ORDER BY
                [wip].[Value]")
        {
        }

        public int OrgId { get; set; }
        public int UserId { get; set; }
        public int MilestoneId { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new WorkItemEstimateHours() { OrgId = 0, UserId = 0, MilestoneId = 0 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}