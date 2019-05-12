using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class Labels : Query<Label>
	{
		public Labels() : base(
            @"SELECT * FROM [dbo].[Label] 
            WHERE [OrganizationId]=@orgId AND [IsActive]=@isActive 
            ORDER BY [Name]")
		{
		}

		public int OrgId { get; set; }
		public bool IsActive { get; set; }
	}
}