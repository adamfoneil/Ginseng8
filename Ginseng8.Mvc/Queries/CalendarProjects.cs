using System.Collections.Generic;
using System.Data;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class CalendarProjectsResult
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class CalendarProjects : Query<CalendarProjectsResult>, ITestableQuery
    {
        public CalendarProjects() : base(
            @"SELECT
                [t].[Name] AS [TeamName],
                [wi].[TeamId],
                [app].[Name] AS [ApplicationName],
                COALESCE([wi].[ApplicationId], 0) AS [ApplicationId],
                [prj].[Name] AS [ProjectName],
                [wi].[ProjectId],
                MONTH([ms].[Date]) AS [Month],
                YEAR([ms].[Date]) AS [Year]
            FROM
                [dbo].[Project] [prj]
                INNER JOIN [dbo].[WorkItem] [wi] ON [prj].[Id]=[wi].[ProjectId]
                INNER JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
                LEFT JOIN [dbo].[Application] [app] ON [wi].[ApplicationId]=[app].[Id] AND [app].[IsActive]=1
                INNER JOIN [dbo].[Team] [t] ON [wi].[TeamId]=[t].[Id]
            WHERE
                [wi].[OrganizationId]=@orgId AND                
                [prj].[IsActive]=1 AND
                [t].[IsActive]=1 AND
                ([ms].[Date]>getdate() OR ([ms].[Date]<getdate() AND (EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL))))
                {andWhere}
            GROUP BY
                [t].[Name],
                [wi].[TeamId],
                [app].[Name],
                COALESCE([wi].[ApplicationId], 0),
                [wi].[ProjectId],
                [prj].[Name],
                MONTH([ms].[Date]),
                YEAR([ms].[Date])")
        {
        }

        public int OrgId { get; set; }

        [Where("[wi].[TeamId]=@teamId")]
        public int? TeamId { get; set; }

        [Where("[wi].[ApplicationId]=@appId")]
        public int? AppId { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new CalendarProjects() { OrgId = 1 };
            yield return new CalendarProjects() { OrgId = 1, TeamId = 1 };
            yield return new CalendarProjects() { OrgId = 1, AppId = 1 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}