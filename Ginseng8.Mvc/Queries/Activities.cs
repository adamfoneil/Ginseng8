using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class Activities : Query<Activity>
	{
		public Activities() : base("SELECT * FROM [dbo].[Activity] WHERE [OrganizationId]=@orgId AND [IsActive]=@isActive ORDER BY [Order], [Name]")
		{
		}

		public int OrgId { get; set; }
		public bool IsActive { get; set; }
	}
}