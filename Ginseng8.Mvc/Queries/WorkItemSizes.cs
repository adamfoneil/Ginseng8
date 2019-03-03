using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class WorkItemSizes : Query<WorkItemSize>
	{
		public WorkItemSizes() : base(
			@"SELECT *
			FROM [dbo].[WorkItemSize]
			WHERE [OrganizationId]=@orgId
			ORDER BY [EstimateHours]")
		{
		}

		public int OrgId { get; set; }
	}
}