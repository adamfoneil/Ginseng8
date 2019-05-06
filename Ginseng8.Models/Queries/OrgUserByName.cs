using System.Collections.Generic;
using System.Data;
using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Interfaces;

namespace Ginseng.Models.Queries
{
	public class OrgUserByName : Query<OrganizationUser>, ITestableQuery
	{
		public OrgUserByName() : base(
			@"SELECT [ou].*, [u].[UserName], [u].[Email], [u].[PhoneNumber]
			FROM [dbo].[OrganizationUser] [ou]
			INNER JOIN [dbo].[AspNetUsers] [u] ON [ou].[UserId]=[u].[UserId]
			WHERE
				[ou].[OrganizationId]=@orgId AND
				[ou].[IsEnabled]=1 AND (
					[u].[Email] LIKE '%' + @name + '%' OR
					[u].[UserName] LIKE '%' + @name + '%' OR
					[ou].[DisplayName] LIKE '%' + @name + '%'
				)")
		{
		}

		public int OrgId { get; set; }
		public string Name { get; set; }

		public IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new OrgUserByName() { OrgId = 0, Name = "anyone" };
		}

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}
	}
}