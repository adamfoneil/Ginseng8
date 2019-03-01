using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class Milestones : Query<Milestone>
	{
		public Milestones() : base(
			@"SELECT * FROM [dbo].[Milestone]
			WHERE [OrganizationId]=@orgId AND [Date]>=DATEADD(d, -7, getdate())
			ORDER BY [Date]")
		{
		}

		public int OrgId { get; set; }
	}
}