using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dapper.QX;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class AllMilestonesResult
    {
        public int? TeamId { get; set; }        
        public int Value { get; set; }
        public string Text { get; set; }

        public SelectListItem ToSelectListItem()
        {
            return new SelectListItem() { Value = Value.ToString(), Text = Text };
        }
    }

    public class AllMilestones : Query<AllMilestonesResult>, ITestableQuery
    {
        public AllMilestones() : base(
            @"SELECT
                [ms].[TeamId], [ms].[Id] AS [Value], [ms].[Name] + ': ' + FORMAT([ms].[Date], 'ddd M/d') AS [Text]
            FROM
				[dbo].[Milestone] [ms]
                LEFT JOIN [dbo].[Team] [t] ON [ms].[TeamId]=[t].[Id]                
			WHERE
				[ms].[OrganizationId]=@orgId AND
				[ms].[Date]>DATEADD(d, -7, getdate())
			ORDER BY
				[ms].[Date]")
        {
        }

        public int OrgId { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new AllMilestones() { OrgId = 1 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}