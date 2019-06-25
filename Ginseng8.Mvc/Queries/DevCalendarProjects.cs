using System;
using System.Collections.Generic;
using System.Data;
using Ginseng.Mvc.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class DevCalendarProjectsResult
    {
        public string TeamName { get; set; }
        public int TeamId { get; set; }
        public string ApplicationName { get; set; }
        public int ApplicationId { get; set; }
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public string DeveloperName { get; set; }
        public int DeveloperUserId { get; set; }        
        public int EstimateHours { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public DateTime GetMonthStartDate() => new DateTime(Year, Month, 1);
        public DateTime GetMonthEndDate() => new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));
    }

    public class DevCalendarProjects : Query<DevCalendarProjectsResult>, ITestableQuery
    {
        public DevCalendarProjects() : base(
            @"WITH [source] AS (
                SELECT
                    [t].[Name] AS [TeamName],
                    [wi].[TeamId],
                    [app].[Name] AS [ApplicationName],
                    COALESCE([wi].[ApplicationId], 0) AS [ApplicationId],
                    [prj].[Name] AS [ProjectName],
                    [wi].[ProjectId],
                    COALESCE([ou].[DisplayName], [u].[UserName], '(unassigned)') AS [DeveloperName],
                    COALESCE([wi].[DeveloperUserId], 0) AS [DeveloperUserId],
                    COUNT(1) AS [OpenWorkItems],
                    SUM(COALESCE([wid].[EstimateHours], [sz].[EstimateHours])) AS [EstimateHours],
                    MONTH([ms].[Date]) AS [Month],
                    YEAR([ms].[Date]) AS [Year]                
                FROM
                    [dbo].[Project] [prj]
                    INNER JOIN [dbo].[WorkItem] [wi] ON [prj].[Id]=[wi].[ProjectId]
                    INNER JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
                    LEFT JOIN [dbo].[Application] [app] ON [wi].[ApplicationId]=[app].[Id] AND [app].[IsActive]=1
                    INNER JOIN [dbo].[Team] [t] ON [wi].[TeamId]=[t].[Id]
                    LEFT JOIN [dbo].[AspNetUsers] [u] ON [wi].[DeveloperUserId]=[u].[UserId]
                    LEFT JOIN [dbo].[OrganizationUser] [ou] ON 
                        [wi].[OrganizationId]=[ou].[OrganizationId] AND
                        [wi].[DeveloperUserId]=[ou].[UserId]
                    LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
                    LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
                WHERE
                    [wi].[OrganizationId]=@orgId AND                
                    [prj].[IsActive]=1 AND
                    [t].[IsActive]=1 AND
                    ([ms].[Date]>getdate() OR ([ms].[Date]<getdate() AND (EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL)))) AND
                    [wi].[CloseReasonId] IS NULL 
                    {andWhere}
                GROUP BY
                    [t].[Name],
                    [wi].[TeamId],
                    [app].[Name],
                    COALESCE([wi].[ApplicationId], 0),
                    [wi].[ProjectId],
                    [prj].[Name],
                    COALESCE([ou].[DisplayName], [u].[UserName], '(unassigned)'),
                    COALESCE([wi].[DeveloperUserId], 0),
                    MONTH([ms].[Date]),
                    YEAR([ms].[Date])
            ) SELECT 
                [src].*
            FROM
                [source] [src]
            ORDER BY
                [Year],
                [Month]")
        {
        }

        public int OrgId { get; set; }

        [Where("[wi].[TeamId]=@teamId")]
        public int? TeamId { get; set; }

        [Where("[wi].[ApplicationId]=@appId")]
        public int? AppId { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new DevCalendarProjects() { OrgId = 1 };
            yield return new DevCalendarProjects() { OrgId = 1, TeamId = 1 };
            yield return new DevCalendarProjects() { OrgId = 1, AppId = 1 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}