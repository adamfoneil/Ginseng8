using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class MySwitchOrgs : Query<Organization>
	{
		public MySwitchOrgs() : base(
			@"SELECT [org].*
			FROM [dbo].[Organization] [org]
			WHERE (
				EXISTS(SELECT 1 FROM [dbo].[OrganizationUser] WHERE [UserId]=@userId AND [IsEnabled]=1 AND [OrganizationId]=[org].[Id]) OR
				[org].[OwnerUserId]=@userId
			) AND [org].[Id]<>@currentOrgId")
		{
		}

		public int UserId { get; set; }
		public int CurrentOrgId { get; set; }
	}
}