using Ginseng.Mvc.Queries.Models;
using Dapper.QX;
using Dapper.QX.Attributes;
using Dapper.QX.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
    public class AllPendingWorkLogs : Query<WorkLogsResult>, ITestableQuery
    {
        public AllPendingWorkLogs() : base(
            @"SELECT
				[wl].*,
                [t].[Name] AS [TeamName],
                [wi].[ApplicationId],
                [app].[Name] AS [ApplicationName],
                COALESCE([ou].[DisplayName], [u].[UserName]) AS [DeveloperName],
				CASE
					WHEN [wl].[ProjectId] IS NOT NULL THEN [p].[Name]
					WHEN [wl].[WorkItemId] IS NOT NULL THEN [wi].[Title]
				END AS [Title],
				CONVERT(bit, CASE
					WHEN [wl].[ProjectId] IS NOT NULL THEN 1
					ELSE 0
				END) AS [IsProject],
                COALESCE([wl].[WorkItemId], [wl].[ProjectId]) AS [ItemId],
				[wi].[Number] AS [WorkItemNumber],
				DATEPART(yyyy, [wl].[Date]) AS [Year],
				DATEPART(ww, [wl].[Date]) AS [WeekNumber],
                [ou].[InvoiceRate],
                [ou].[InvoiceRate]*[wl].[Hours] AS [Amount]
			FROM
				[dbo].[PendingWorkLog] [wl]
				LEFT JOIN [dbo].[WorkItem] [wi] ON [wl].[WorkItemId]=[wi].[Id]                
				LEFT JOIN [dbo].[Project] [p] ON [wl].[ProjectId]=[p].[Id]
                LEFT JOIN [dbo].[Application] [app] ON [wl].[ApplicationId]=[app].[Id]
                LEFT JOIN [dbo].[Team] [t] ON [wi].[TeamId]=[t].[Id]
                INNER JOIN [dbo].[OrganizationUser] [ou] ON [wl].[UserId]=[ou].[UserId] AND [wl].[OrganizationId]=[ou].[OrganizationId]
                INNER JOIN [dbo].[AspNetUsers] [u] ON [wl].[UserId]=[u].[UserId]
			WHERE
				[wl].[OrganizationId]=@orgId {andWhere}")
        {
        }

        public int OrgId { get; set; }

        [Where("[wl].[UserId]=@userId")]
        public int? UserId { get; set; }

        [Where("[app].[Id]=@appId")]
        public int? AppId { get; set; }

        [Where("DATEPART(yyyy, [wl].[Date])=@year")]
        public int? Year { get; set; }

        [Where("DATEPART(ww, [wl].[Date])=@weekNumber")]
        public int? WeekNumber { get; set; }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new AllPendingWorkLogs() { OrgId = 0 };
            yield return new AllPendingWorkLogs() { Year = 2019 };
            yield return new AllPendingWorkLogs() { WeekNumber = 1 };
            yield return new AllPendingWorkLogs() { UserId = 1 };
        }
    }
}