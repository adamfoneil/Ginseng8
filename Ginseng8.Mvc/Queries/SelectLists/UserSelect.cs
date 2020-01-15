using Ginseng.Mvc.Classes;
using Dapper.QX.Attributes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class UserSelect : SelectListQuery
	{
		public UserSelect() : base(
			@"SELECT [u].[UserId] AS [Value], COALESCE([ou].[DisplayName], [u].[UserName]) AS [Text]
			FROM [dbo].[AspNetUsers] [u]
			INNER JOIN [dbo].[OrganizationUser] [ou] ON [u].[UserId]=[ou].[UserId]
			WHERE [ou].[OrganizationId]=@orgId {andWhere}
			ORDER BY COALESCE([ou].[DisplayName], [u].[UserName]) ASC")
		{
		}

		public int OrgId { get; set; }

        [Where("[ou].[IsEnabled]=@isEnabled")]
        public bool? IsEnabled { get; set; }
	}
}