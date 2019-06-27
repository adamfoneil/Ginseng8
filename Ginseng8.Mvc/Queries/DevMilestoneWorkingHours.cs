using System;
using System.Collections.Generic;
using System.Data;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class DevMilestoneWorkingHoursResult
    {
        public int ApplicationId { get; set; }
        public int MilestoneId { get; set; }
        public int DeveloperId { get; set; }
        public string DeveloperName { get; set; }
        public int WorkingHours { get; set; }
        public int EstimateHours { get; set; }
        public int AvailableHours { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class DevMilestoneWorkingHours : Query<DevMilestoneWorkingHoursResult>, ITestableQuery
    {
        public DevMilestoneWorkingHours() : base(
            @"WITH [estimates] AS (
                SELECT
                    [wi].[ApplicationId],
                    [wi].[DeveloperUserId],
                    [wi].[MilestoneId],                    
                    SUM(COALESCE([wid].[EstimateHours], [sz].[EstimateHours])) AS [EstimateHours]
                FROM
                    [dbo].[WorkItem] [wi]
                    LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
                    LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
                WHERE
                    [wi].[DeveloperUserId] IS NOT NULL AND
                    [wi].[MilestoneId] IS NOT NULL AND
                    COALESCE([wid].[EstimateHours], [sz].[EstimateHours]) IS NOT NULL AND
                    [wi].[CloseReasonId] IS NULL
                GROUP BY
                    [wi].[ApplicationId],
                    [wi].[DeveloperUserId],
                    [wi].[MilestoneId]    
            ) SELECT
                [ms].[ApplicationId],
                [dm].[MilestoneId],
                [dm].[DeveloperId], 
                COALESCE([ou].[DisplayName], [u].[UserName]) AS [DeveloperName],
                SUM([wd].[Hours]) AS [WorkingHours],
                [e].[EstimateHours],
                SUM([wd].[Hours]) - [e].[EstimateHours] AS [AvailableHours],
                YEAR([ms].[Date]) AS [Year],
                MONTH([ms].[Date]) AS [Month]
            FROM 
                [dbo].[DeveloperMilestone] [dm]
                INNER JOIN [dbo].[Milestone] [ms] ON [dm].[MilestoneId]=[ms].[Id]
                INNER JOIN [dbo].[Application] [app] ON [ms].[ApplicationId]=[app].[Id]
                CROSS APPLY [dbo].[FnWorkingDays](@orgId, dbo.MaxDate([dm].[StartDate], @localDate), [ms].[Date]) [wd]
                LEFT JOIN [estimates] [e] ON 
                    [dm].[DeveloperId]=[e].[DeveloperUserId] AND
                    [dm].[MilestoneId]=[e].[MilestoneId] AND
                    [ms].[ApplicationId]=[e].[ApplicationId]
                INNER JOIN [dbo].[OrganizationUser] [ou] ON
                    [dm].[DeveloperId]=[ou].[UserId] AND [ou].[OrganizationId]=@orgId                    
                INNER JOIN [dbo].[AspNetUsers] [u] ON [ou].[UserId]=[u].[UserId]
            WHERE 
                [wd].[UserId]=[dm].[DeveloperId] AND
                [app].[OrganizationId]=@orgId {andWhere}
            GROUP BY
                [ms].[ApplicationId],
                [dm].[MilestoneId],
                [dm].[DeveloperId],
                [ou].[DisplayName], [u].[UserName],
                [e].[EstimateHours],
                YEAR([ms].[Date]),
                MONTH([ms].[Date])")
        {
        }

        public int OrgId { get; set; }

        /// <summary>
        /// User's local date for calculating correct working days
        /// </summary>
        public DateTime LocalDate { get; set; } = DateTime.Today;

        [Where("[dm].[MilestoneId]=@milestoneId")]
        public int? MilestoneId { get; set; }

        [Where("[app].[Id]=@appId")]
        public int? AppId { get; set; }

        [Where("[dm].[DeveloperId]=@userId")]
        public int? UserId { get; set; }

        [Where("[ms].[Date]>=@startMilestoneDate")]
        public DateTime? StartMilestoneDate { get; set; }

        [Where("[ms].[Date]<=@endMilestoneDate")]
        public DateTime? EndMilestoneDate { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new DevMilestoneWorkingHours() { OrgId = 0 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}
