using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
    public class MilestoneMetricsResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TotalWorkItems { get; set; }
        public int CompletedWorkItems { get; set; }
        public double PercentComplete { get; set; }
    }

    public class MilestoneMetrics : Query<MilestoneMetricsResult>, ITestableQuery
    {
        public MilestoneMetrics() : base(
            @"WITH [source] AS (
                SELECT
                    [ms].[Id],
                    [ms].[Name],
                    (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id]) AS [TotalWorkItems],
                    (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NOT NULL) AS [CompletedWorkItems]
                FROM
                    [dbo].[Milestone] [ms]
                WHERE
                    [ms].[OrganizationId]=@orgId AND
                    [ms].[Id] IN @milestoneIds
            ) SELECT
                [src].*,
                CASE
                    WHEN [TotalWorkItems] > 0 THEN CONVERT(float, [CompletedWorkItems]) / CONVERT(float, [TotalWorkItems])
                    ELSE 0
                END AS [PercentComplete]
            FROM
                [source] [src]")
        {
        }

        public int OrgId { get; set; }
        
        public int[] MilestoneIds { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new MilestoneMetrics() { OrgId = 0, MilestoneIds = new int[] { 1, 2, 3 } };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecute(connection);
        }
    }
}