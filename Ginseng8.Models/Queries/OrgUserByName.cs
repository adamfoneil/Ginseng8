using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Models.Queries
{
    public class OrgUserByName : Query<OrganizationUser>, ITestableQuery
    {
        public OrgUserByName() : base(
            @"SELECT [ou].*, [u].[UserName], [u].[Email], [u].[PhoneNumber]
			FROM [dbo].[OrganizationUser] [ou]
			INNER JOIN [dbo].[AspNetUsers] [u] ON [ou].[UserId]=[u].[UserId]
			{where}")
        {
        }

        [Where("[ou].[OrganizationId]=@orgId")]
        public int? OrgId { get; set; }

        [Where("[u].[UserName]=@userName")]
        public string UserName { get; set; }

        [Where(@"(
            [u].[Email] LIKE '%' + @searchName + '%' OR
            [u].[UserName] LIKE '%' + @searchName + '%' OR
            [ou].[DisplayName] LIKE '%' + @searchName + '%'
        )")]
        public string SearchName { get; set; }

        [Where("[ou].[IsEnabled]=@isEnabled")]
        public bool? IsEnabled { get; set; } = true;

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new OrgUserByName() { OrgId = 0, SearchName = "anyone" };
            yield return new OrgUserByName() { OrgId = 0, UserName = "anyone" };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}