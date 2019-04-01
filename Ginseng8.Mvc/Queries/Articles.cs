using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class Articles : Query<Article>
	{
		public Articles() : base(
			@"SELECT 
				[a].*
			FROM 
				[dbo].[Article] [a]
			WHERE 
				[a].[OrganizationId]=@orgId AND 
				[a].[IsActive]=@isActive
			ORDER BY 
				[Title]")
		{
		}

		public int OrgId { get; set; }
		public bool IsActive { get; set; }
	}
}