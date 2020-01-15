using Ginseng.Models;
using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
	public class WorkItemSizes : Query<WorkItemSize>
	{
		public WorkItemSizes() : base(
			@"SELECT [wis].*, [gp].[ColorGradientPosition]
			FROM [dbo].[WorkItemSize] [wis] INNER JOIN [dbo].[FnColorGradientPositions](@orgId) [gp] ON [wis].[Id]=[gp].[Id]
			WHERE [wis].[OrganizationId]=@orgId")
		{
		}

		public int OrgId { get; set; }
	}
}