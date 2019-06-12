using Ginseng.Mvc.Interfaces;
using Postulate.Base;
using System;

namespace Ginseng.Mvc.Queries
{
    public class AppInfoResult : IItemMetrics
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DateModified { get; set; }
        public string InvoiceEmail { get; set; }
        public bool AllowNewItems { get; set; }
        public int? EstimateHours { get; set; }
        public int? TotalWorkItems { get; set; }
        public int? OpenWorkItems { get; set; }
        public int? ClosedWorkItems { get; set; }
        public int? UnestimatedWorkItems { get; set; }
        public int? StoppedWorkItems { get; set; }
        public int? UnscheduledWorkItems { get; set; }
        public int? ImpededWorkItems { get; set; }
        public int AllowDelete { get; set; }
        public double? PercentComplete { get; set; }

        public bool HasModifiers()
        {
            return
                ImpededWorkItems > 0 ||
                UnestimatedWorkItems > 0 ||
                StoppedWorkItems > 0;
        }
    }

    public class AppInfo : Query<AppInfoResult>
    {
        public AppInfo() : base(
            @"WITH [source] AS (
                SELECT
                    [app].*,
                    (SELECT
                        SUM(COALESCE([wid].[EstimateHours], [sz].[EstimateHours]))
                        FROM [dbo].[WorkItem] [wi] LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
                        LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
                        WHERE [wi].[ApplicationId]=[app].[Id] AND [wi].[CloseReasonId] IS NULL) AS [EstimateHours],
                        (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ApplicationId]=[app].[Id]) AS [TotalWorkItems],
                    (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ApplicationId]=[app].[Id] AND [CloseReasonId] IS NULL) AS [OpenWorkItems],
                    (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ApplicationId]=[app].[Id] AND [CloseReasonId] IS NOT NULL) AS [ClosedWorkItems],
                    (SELECT COUNT(1)
                        FROM
                            [dbo].[WorkItem] [wi]
                            LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
                            LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
                        WHERE
                            [ProjectId]=[app].[Id] AND [CloseReasonId] IS NULL AND
                            COALESCE([wid].[EstimateHours], [sz].[EstimateHours]) IS NULL) AS [UnestimatedWorkItems],
                    (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ApplicationId]=[app].[Id] AND [CloseReasonId] IS NULL AND [MilestoneId] IS NOT NULL AND [ActivityId] IS NULL) AS [StoppedWorkItems],
                    (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ApplicationId]=[app].[Id] AND [CloseReasonId] IS NULL AND [MilestoneId] IS NULL) AS [UnscheduledWorkItems],
                    (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ApplicationId]=[app].[Id] AND [CloseReasonId] IS NULL AND [HasImpediment]=1) AS [ImpededWorkItems],
                    CASE
                        WHEN EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [ApplicationId]=[app].[Id]) THEN 0            
                        ELSE 1
                    END AS [AllowDelete]
                FROM
                    [dbo].[Application] [app]
                WHERE
                    [app].[OrganizationId]=@orgId        
            ) SELECT
                [source].*,
                CASE
                    WHEN [TotalWorkItems] > 0 THEN CONVERT(float, [ClosedWorkItems]) / CONVERT(float, [TotalWorkItems])
                    ELSE 0
                END AS [PercentComplete]
            FROM
                [source]")
        {
        }
    }
}