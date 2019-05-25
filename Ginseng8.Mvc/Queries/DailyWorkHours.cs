using Postulate.Base;
using Postulate.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
    public class DailyHoursResult
    {
        public DateTime Date { get; set; }
        public int AvailableHours { get; set; }
        public int PlannedHours { get; set; }
        public double PercentFull { get; set; }
    }

    public class DailyWorkHours : Query<DailyHoursResult>, ITestableQuery
    {
        public DailyWorkHours() : base(
            @"WITH [dates] AS (
                SELECT
                    [wd].[Date], [wd].[Hours]
                FROM
                    [dbo].[FnWorkingDays](@orgId, @startDate, @endDate) [wd]
                WHERE
                    [UserId]=@userId
            ), [estimates] AS (
                SELECT      
                    [wi].[Date],
                    SUM(COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0)) AS [Hours]     
                FROM
                    [dbo].[WorkItem] [wi]
                    LEFT JOIN [dbo].[WorkItemPriority] [wip] ON [wi].[Id]=[wip].[WorkItemId]
                    LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
                    LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
                WHERE
                    [wi].[OrganizationId]=@orgId AND        
                    [wi].[DeveloperUserId]=@userId AND
                    [wi].[CloseReasonId] IS NULL
                GROUP BY
                    [wi].[Date]
            ) SELECT
                [d].[Date],
                [d].[Hours] AS [AvailableHours],
                ISNULL([e].[Hours], 0) AS [PlannedHours],
                CASE
                    WHEN [d].[Hours] > 0 THEN CONVERT(float, ISNULL([e].[Hours], 0)) / CONVERT(float, [d].[Hours])
                    ELSE 0
                END AS [PercentFull]
            FROM
                [dates] [d]
                LEFT JOIN [estimates] [e] ON [d].[Date]=[e].[Date]")
        {
        }

        public int OrgId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new DailyWorkHours() { OrgId = 0, UserId = 0, StartDate = new DateTime(2019, 5, 25), EndDate = new DateTime(2019, 5, 31) };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}