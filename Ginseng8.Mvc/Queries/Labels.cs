using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
    public class Labels : Query<Label>, ITestableQuery
    {
        public Labels() : base(
            @"SELECT *
            FROM [dbo].[Label]
            WHERE
                [OrganizationId]=@orgId AND
                [IsActive]=@isActive
            ORDER BY [Name]")
        {
        }

        public int OrgId { get; set; }
        public bool IsActive { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new Labels() { OrgId = 0, IsActive = true };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}