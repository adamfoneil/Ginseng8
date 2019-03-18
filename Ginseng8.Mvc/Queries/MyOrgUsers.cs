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
				[org].[Name] AS [OrgName]
			FROM 
				[dbo].[OrganizationUser] [ou]
				INNER JOIN [dbo].[Organization] [org] ON [ou].[OrganizationId]=[org].[Id]
			WHERE 
				[UserId]=@userId {andWhere}")
		{
		}

		public int UserId { get; set; }

		[Where("[ou].[IsRequest]=@isRequest")]
		public bool? IsRequest { get; set; }		

		[Where("[ou].[IsEnabled]=@isEnabled")]
		public bool? IsEnabled { get; set; }
	}
}