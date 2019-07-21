using Ginseng.Mvc.Interfaces;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class CalendarProjectMetricsResult : IItemMetrics
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int ProjectId { get; set; }

        public int? TotalWorkItems { get; set; }
        public int? OpenWorkItems { get; set; }
        public int? EstimateHours { get; set; }
        public int? UnestimatedWorkItems { get; set; }
        public int? StoppedWorkItems { get; set; }
        public int? ImpededWorkItems { get; set; }
        public double PercentComplete { get; set; }

        public bool HasModifiers()
        {
            return
                ImpededWorkItems > 0 ||
                UnestimatedWorkItems > 0 ||
                StoppedWorkItems > 0;
        }
    }

    public class CalendarProjectMetrics : Query<CalendarProjectMetricsResult>
    {
        public CalendarProjectMetrics() : base(
            @"WITH [source] AS (
                SELECT
                    [wi].[OrganizationId],
                    YEAR([ms].[Date]) AS [Year],
                    MONTH([ms].[Date]) AS [Month],
                    [prj].[Id] AS [ProjectId],
                    COUNT(1) AS [TotalWorkItems]
                FROM
                    [dbo].[Project] [prj]
                    INNER JOIN [dbo].[WorkItem] [wi] ON [prj].[Id]=[wi].[ProjectId]
                    INNER JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
                WHERE
                    [wi].[OrganizationId]=@orgId
                GROUP BY
                    [wi].[OrganizationId],
                    YEAR([ms].[Date]),
                    MONTH([ms].[Date]),
                    [prj].[Id]
            ), [metrics] AS (
	            SELECT
		            [OrganizationId],
		            [Year],
		            [Month],
		            [ProjectId],
		            [TotalWorkItems],
		            (
			            SELECT 
				            COUNT(1) 
			            FROM 
				            [dbo].[WorkItem] [wi]
				            INNER JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
			            WHERE 
				            [ProjectId]=[src].[ProjectId] AND 
				            [CloseReasonId] IS NOT NULL AND 
				            YEAR([ms].[Date])=[src].[Year] AND 
				            MONTH([ms].[Date])=[src].[Month]
		            ) AS [ClosedWorkItems],
		            (
			            SELECT
				            COUNT(1) 
			            FROM
				            [dbo].[WorkItem] [wi] 
				            INNER JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
			            WHERE 
				            [ProjectId]=[src].[ProjectId] AND 
				            YEAR([ms].[Date])=[src].[Year] AND 
				            MONTH([ms].[Date])=[src].[Month] AND
				            [wi].[CloseReasonId] IS NULL
		            ) AS [OpenWorkItems],
		            (
			            SELECT            
				            SUM(COALESCE([wid].[EstimateHours], [sz].[EstimateHours]))
			            FROM 
				            [dbo].[WorkItem] [wi] 
				            INNER JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
				            LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
				            LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
			            WHERE
				            [ProjectId]=[src].[ProjectId] AND 
				            YEAR([ms].[Date])=[src].[Year] AND 
				            MONTH([ms].[Date])=[src].[Month] AND
				            [wi].[CloseReasonId] IS NULL
		            ) AS [EstimateHours],
		            (
			            SELECT
				            COUNT(1) 
			            FROM
				            [dbo].[WorkItem] [wi] 
				            INNER JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
				            LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
				            LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
			            WHERE 
				            [ProjectId]=[src].[ProjectId] AND 
				            YEAR([ms].[Date])=[src].[Year] AND 
				            MONTH([ms].[Date])=[src].[Month] AND
				            [wi].[CloseReasonId] IS NULL AND
				            COALESCE([wid].[EstimateHours], [sz].[EstimateHours]) IS NULL
		            ) AS [UnestimatedWorkItems],
		            (
			            SELECT 
				            COUNT(1) 
			            FROM 
				            [dbo].[WorkItem] [wi]
				            INNER JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
			            WHERE 
				            [CloseReasonId] IS NULL AND
				            [ActivityId] IS NULL AND
				            [ProjectId]=[src].[ProjectId] AND
				            YEAR([ms].[Date])=[src].[Year] AND 
				            MONTH([ms].[Date])=[src].[Month]
		            ) AS [StoppedWorkItems],
		            (
			            SELECT 
				            COUNT(1) 
			            FROM 
				            [dbo].[WorkItem] [wi]
				            INNER JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
			            WHERE 
				            [CloseReasonId] IS NULL AND
				            [HasImpediment]=1 AND
				            [ProjectId]=[src].[ProjectId] AND
				            YEAR([ms].[Date])=[src].[Year] AND 
				            MONTH([ms].[Date])=[src].[Month]
		            ) AS [ImpededWorkItems]        
	            FROM
		            [source] [src]
            ) SELECT 
	            [m].*,
	            CASE
                    WHEN [TotalWorkItems] > 0 THEN CONVERT(float, [ClosedWorkItems]) / CONVERT(float, [TotalWorkItems])
                    ELSE 0
                END AS [PercentComplete]
            FROM
	            [metrics] [m]")
        {
        }

        public int OrgId { get; set; }
    }
}