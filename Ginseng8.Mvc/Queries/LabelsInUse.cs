using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class LabelsInUse : Query<Label>
	{
		public LabelsInUse() : base(
			@"SELECT
				[wil].[WorkItemId],
				[l].*
			FROM
				[dbo].[WorkItemLabel] [wil]
				INNER JOIN [dbo].[Label] [l] ON [wil].[LabelId]=[l].[Id]
				INNER JOIN [dbo].[WorkItem] [wi] ON [wil].[WorkItemId]=[wi].[Id]
			WHERE
				[wil].[WorkItemId] IN @workItemIds AND
				[wi].[OrganizationId]=@orgId
			ORDER BY
				[wil].[WorkItemId],
				[l].[Name]")
		{
		}

		public int OrgId { get; set; }
		public int[] WorkItemIds { get; set; }
	}
}