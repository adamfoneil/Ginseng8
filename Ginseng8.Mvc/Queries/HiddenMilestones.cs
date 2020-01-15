using System.Collections.Generic;
using System.Data;
using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class HiddenMilestones: Query<Milestone>, ITestableQuery
    {
        public HiddenMilestones() : base(
            @"SELECT
	            [ms].[Id],
	            [ms].[Name],
	            [ms].[Date],
	            [ms].[CreatedBy],
	            [ms].[DateCreated],
	            [ms].[ModifiedBy],
	            [ms].[DateModified],
	            [ms].[TeamId],
	            [ms].[OrganizationId],
                DATEDIFF(d, getdate(), [ms].[Date]) AS [MilestoneDaysAway]
            FROM
	            [dbo].[Milestone] [ms]
	            INNER JOIN [dbo].[MilestoneUserView] [muv] ON [ms].[Id]=[muv].[MilestoneId]
            WHERE
	            [muv].[UserId]=@userId AND
	            [ms].[OrganizationId]=@orgId AND
	            [muv].[IsVisible]=0
            UNION
            SELECT
	            0, 
	            '(no milestone)' AS [Name],
	            '12/31/9999' AS [Date],                
	            [muv].[CreatedBy],
	            [muv].[DateCreated],
	            [muv].[ModifiedBy],
	            [muv].[DateModified],
	            NULL AS [TeamId],
	            @orgId AS [OrganizationId],
                NULL AS [MilestoneDaysAway]
            FROM
	            [dbo].[MilestoneUserView] [muv]
            WHERE
	            [muv].[UserId]=@userId AND
	            [muv].[MilestoneId]=0 AND
	            [muv].[IsVisible]=0
            ORDER BY [Date]")
        {
        }

        public int OrgId { get; set; }
        public int UserId { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new HiddenMilestones() { OrgId = 1, UserId = 10 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}
