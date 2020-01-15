using System.Collections.Generic;
using System.Data;
using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class TeamLabels : Query<Label>, ITestableQuery
    {
        public TeamLabels() : base(
            @"SELECT
                [tl].[TeamId],
                [lbl].*
            FROM
                [dbo].[TeamLabel] [tl]
                INNER JOIN [dbo].[Label] [lbl] ON [tl].[LabelId]=[lbl].[Id]
            WHERE
                [lbl].[OrganizationId]=@orgId AND
                [tl].[TeamId]=@teamId")
        {
        }

        public int OrgId { get; set; }
        public int TeamId { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new TeamLabels() { OrgId = 1, TeamId = 1 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}