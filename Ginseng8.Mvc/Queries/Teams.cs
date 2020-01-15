using System.Collections.Generic;
using System.Data;
using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;
using Dapper.QX.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class Teams : Query<Team>, ITestableQuery
    {
        public Teams() : base(
            @"SELECT [t].*
            FROM [dbo].[Team] [t]
            WHERE [OrganizationId]=@orgId {andWhere} 
            ORDER BY [Name]")
        {
        }

        public int OrgId { get; set; }

        [Where("[IsActive]=@isActive")]
        public bool? IsActive { get; set; } = true;

        [Where("[Id]=@id")]
        public int? Id { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new Teams() { OrgId = 1 };
            yield return new Teams() { IsActive = true };
            yield return new Teams() { Id = 0 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}