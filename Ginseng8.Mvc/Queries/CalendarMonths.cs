using System.Collections.Generic;
using System.Data;
using Ginseng.Mvc.Classes;
using Dapper.QX;
using Dapper.QX.Attributes;
using Dapper.QX.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class MilestoneMonths : Query<YearMonth>, ITestableQuery
    {
        public MilestoneMonths() : base(
            @"SELECT
                YEAR([ms].[Date]) AS [Year],
                MONTH([ms].[Date]) AS [Month],
                ROW_NUMBER() OVER (ORDER BY YEAR([ms].[Date]), MONTH([ms].[Date])) AS [Index]
            FROM
                [dbo].[Milestone] [ms]
                INNER JOIN [dbo].[Team] [t] ON [ms].[TeamId]=[t].[Id]
                LEFT JOIN [dbo].[Application] [app] ON [ms].[ApplicationId]=[app].[Id] AND [app].[IsActive]=1
            WHERE
                [t].[OrganizationId]=@orgId AND
                [t].[IsActive]=1 AND
                ([ms].[Date]>getdate() OR ([ms].[Date]<getdate() AND (EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL))))
                {andWhere}
            GROUP BY
                YEAR([ms].[Date]),
                MONTH([ms].[Date])")
        {
        }

        public int OrgId { get; set; }

        [Where("[ms].[TeamId]=@teamId")]
        public int? TeamId { get; set; }

        [Where("[ms].[ApplicationId]=@appId")]
        public int? AppId { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new MilestoneMonths() { OrgId = 0 };
            yield return new MilestoneMonths() { OrgId = 0, AppId = 1 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}