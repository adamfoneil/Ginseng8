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
        public int WorkingHours { get; set; }
        public int EstimateHours { get; set; }
        public int AvailableHours { get; set; }
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
                    COALESCE([wid].[EstimateHours], [sz].[EstimateHours]) IS NOT NULL
                GROUP BY
                    [wi].[ApplicationId],
                    [wi].[DeveloperUserId],
                    [wi].[MilestoneId]    
            ) SELECT
                [ms].[ApplicationId],
                [dm].[MilestoneId],
                [dm].[DeveloperId], 
                SUM([wd].[Hours]) AS [WorkingHours],
                [e].[EstimateHours],
                SUM([wd].[Hours]) - [e].[EstimateHours] AS [AvailableHours]
            FROM 
                [dbo].[DeveloperMilestone] [dm]
                INNER JOIN [dbo].[Milestone] [ms] ON [dm].[MilestoneId]=[ms].[Id]
                INNER JOIN [dbo].[Application] [app] ON [ms].[ApplicationId]=[app].[Id]
                CROSS APPLY [dbo].[FnWorkingDays](@orgId, [dm].[StartDate], [ms].[Date]) [wd]
                LEFT JOIN [estimates] [e] ON 
                    [dm].[DeveloperId]=[e].[DeveloperUserId] AND
                    [dm].[MilestoneId]=[e].[MilestoneId] AND
                    [ms].[ApplicationId]=[e].[ApplicationId]
            WHERE 
                [wd].[UserId]=[dm].[DeveloperId] AND
                [app].[OrganizationId]=@orgId {andWhere}
            GROUP BY
                [ms].[ApplicationId],
                [dm].[MilestoneId],
                [dm].[DeveloperId],
                [e].[EstimateHours]")
        {
        }

        public int OrgId { get; set; }

        [Where("[dm].[MilestoneId]=@milestoneId")]
        public int? MilestoneId { get; set; }

        [Where("[dm].[DeveloperId]=@userId")]
        public int? UserId { get; set; }

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
