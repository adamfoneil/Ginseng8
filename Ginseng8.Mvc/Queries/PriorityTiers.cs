using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class PriorityTiers : Query<PriorityTier>
	{
		public PriorityTiers() : base("SELECT * FROM [dbo].[PriorityTier] WHERE [OrganizationId]=@orgId ORDER BY [Rank]")
		{
		}

		public int OrgId { get; set; }
	}
}