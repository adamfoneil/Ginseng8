using Ginseng.Mvc.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
    public class MilestoneMetricsResult : IItemMetrics
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int? EstimateHours { get; set; }
        public int? TotalWorkItems { get; set; }        
        public int? OpenWorkItems { get; set; }
        public int CompletedWorkItems { get; set; }
        public double PercentComplete { get; set; }
        public int? StoppedWorkItems { get; set; }
        public int? UnestimatedWorkItems { get; set; }
        public int? ImpededWorkItems { get; set; }

        public bool HasModifiers()
        {
            return
                ImpededWorkItems > 0 ||
                UnestimatedWorkItems > 0 ||
                StoppedWorkItems > 0;
        }

    }

    public class MilestoneMetrics : Query<MilestoneMetricsResult>, ITestableQuery
    {
        public MilestoneMetrics() : base(
            @"WITH [source] AS (
                SELECT
                    [ms].[Id],
                    [ms].[Name],                    
                    [ms].[Date],
                    (
                        SELECT 
                            SUM(COALESCE([wid].[EstimateHours], [sz].[EstimateHours])) 
                        FROM 
                            [dbo].[WorkItem] [wi]
                            LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
                            LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
                        WHERE 
                            [wi].[MilestoneId]=[ms].[Id] AND
                            [wi].[CloseReasonId] IS NULL
                    ) AS [EstimateHours],
                    (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id]) AS [TotalWorkItems],
                    (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NOT NULL) AS [CompletedWorkItems],
                    (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL) AS [OpenWorkItems],
                    (SELECT COUNT(1) 
						FROM 
							[dbo].[WorkItem] [wi]
							LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
							LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id] 
						WHERE 
							[MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL AND
							COALESCE([wid].[EstimateHours], [sz].[EstimateHours]) IS NULL) AS [UnestimatedWorkItems],
					(SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL AND [ActivityId] IS NULL) AS [StoppedWorkItems],
					(SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL AND [HasImpediment]=1) AS [ImpededWorkItems]
                FROM
                    [dbo].[Milestone] [ms]
                    INNER JOIN [dbo].[Application] [app] ON [ms].[ApplicationId]=[app].[Id]
                WHERE
                    [app].[OrganizationId]=@orgId AND
                    [ms].[Date] > DATEADD(d, -7, getdate())
                    {andWhere}                    
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

        [Where("[ms].[ApplicationId]=@appId")]
        public int? AppId { get; set; }

        [Where("[ms].[Id] IN @milestoneIds")]
        public int[] MilestoneIds { get; set; }

        [Where("[ms].[Id]=@id")]
        public int? Id { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new MilestoneMetrics() { AppId = 0, MilestoneIds = new int[] { 1, 2, 3 } };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}