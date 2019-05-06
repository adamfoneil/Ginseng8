using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries.Models;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
    public class WorkLogsResult : IWorkItemTitle, IWorkItemNumber
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public int? WorkItemId { get; set; }
        public int ItemId { get; set; } // COALSESCE(WorkItemId, ProjectId)
        public int UserId { get; set; }
        public string DeveloperName { get; set; }
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
        public string TextBody { get; set; }
        public string HtmlBody { get; set; }
        public int? SourceType { get; set; }
        public int? SourceId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DateModified { get; set; }
        public int OrganizationId { get; set; }
        public string Title { get; set; }
        public bool IsProject { get; set; }
        public int? WorkItemNumber { get; set; }
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public decimal InvoiceRate { get; set; }
        public decimal Amount { get; set; }
        public int? InvoiceId { get; set; }

        public int Number => WorkItemNumber ?? 0;
        public string ProjectName => Title;
        public int? ProjectPriority => null;
        public int EstimateHours { get; set; }
        public decimal ColorGradientPosition { get; set; }
        int IWorkItemTitle.ProjectId => ProjectId ?? 0;
        int IWorkItemNumber.Number { get { return WorkItemNumber ?? 0; } set { WorkItemNumber = value; } }

        public bool IsEditable(string userName)
        {
            return false;
        }

        public Week ToWeek()
        {
            return new Week() { Year = Year, WeekNumber = WeekNumber };
        }
    }

    public class PendingWorkLogs : Query<WorkLogsResult>, ITestableQuery
    {
        public PendingWorkLogs() : base(
            @"SELECT
				[wl].*,
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
            yield return new PendingWorkLogs() { OrgId = 0 };
            yield return new PendingWorkLogs() { Year = 2019 };
            yield return new PendingWorkLogs() { WeekNumber = 1 };
            yield return new PendingWorkLogs() { UserId = 1 };
        }
    }
}