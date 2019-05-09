using Ginseng.Mvc.Queries.Models;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
    public class AllInvoiceWorkLogs : Query<WorkLogsResult>, ITestableQuery
    {
        public AllInvoiceWorkLogs() : base(
            @"SELECT
				[iwl].*,
                COALESCE([ou].[DisplayName], [u].[UserName]) AS [DeveloperName],
				CASE
					WHEN [iwl].[ProjectId] IS NOT NULL THEN [p].[Name]
					WHEN [iwl].[WorkItemId] IS NOT NULL THEN [wi].[Title]
				END AS [Title],
				CONVERT(bit, CASE
					WHEN [iwl].[ProjectId] IS NOT NULL THEN 1
					ELSE 0
				END) AS [IsProject],
                COALESCE([iwl].[WorkItemId], [iwl].[ProjectId]) AS [ItemId],
				[wi].[Number] AS [WorkItemNumber],
				DATEPART(yyyy, [iwl].[Date]) AS [Year],
				DATEPART(ww, [iwl].[Date]) AS [WeekNumber],
                [iwl].[Rate] AS [InvoiceRate],
                [iwl].[Amount]
			FROM
				[dbo].[InvoiceWorkLog] [iwl]
				LEFT JOIN [dbo].[WorkItem] [wi] ON [iwl].[WorkItemId]=[wi].[Id]
				LEFT JOIN [dbo].[Project] [p] ON [iwl].[ProjectId]=[p].[Id]
                LEFT JOIN [dbo].[Application] [app] ON [iwl].[ApplicationId]=[app].[Id]
                INNER JOIN [dbo].[OrganizationUser] [ou] ON [iwl].[UserId]=[ou].[UserId] AND [iwl].[OrganizationId]=[ou].[OrganizationId]
                INNER JOIN [dbo].[AspNetUsers] [u] ON [iwl].[UserId]=[u].[UserId]
			WHERE
				[iwl].[OrganizationId]=@orgId {andWhere}")
        {
        }

        public int OrgId { get; set; }

        [Where("[iwl].[InvoiceId]=@invoiceId")]
        public int? InvoiceId { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new AllInvoiceWorkLogs() { OrgId = 0 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}