using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class MyOrgUsers : Query<OrganizationUser>
	{
		public MyOrgUsers() : base(
			@"SELECT 
				[ou].*,
				[org].[Name] AS [OrgName],
				COALESCE([ou].[DisplayName], [u].[UserName]) AS [UserName]
			FROM 
				[dbo].[OrganizationUser] [ou]
				INNER JOIN [dbo].[Organization] [org] ON [ou].[OrganizationId]=[org].[Id]
				INNER JOIN [dbo].[AspNetUsers] [u] ON [ou].[UserId]=[u].[UserId]
			{where}")
		{
		}

		[Case(true, "[org].[OwnerUserId]<>[ou].[UserId]")]
		public bool? ExcludeOwner { get; set; }

		[Where("[ou].[UserId]=@userId")]
		public int? UserId { get; set; }

		[Where("[ou].[OrganizationId]=@orgId")]
		public int? OrgId { get; set; }

		[Where("[ou].[IsRequest]=@isRequest")]
		public bool? IsRequest { get; set; }		

		[Where("[ou].[IsEnabled]=@isEnabled")]
		public bool? IsEnabled { get; set; }

		[Where("[ou].[UserId]<>@excludeUserId")]
		public int? ExcludeUserId { get; set; }
	}
}