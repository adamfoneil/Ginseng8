using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class OpenLabelsResult
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int WorkItemCount { get; set; }
	}

	public class OpenLabels : Query<OpenLabelsResult>
	{
		public OpenLabels() : base(
			@"WITH [source] AS (
				SELECT
					[l].[Id],
					[l].[Name],
					(SELECT COUNT(1)
						FROM [dbo].[WorkItem] [wi]
						WHERE
							EXISTS(SELECT 1 FROM [dbo].[WorkItemLabel] [wil] WHERE [wil].[WorkItemId]=[wi].[Id] AND [wil].[LabelId]=[l].[Id]) AND
							[wi].[CloseReasonId] IS NULL AND
							[wi].[OrganizationId]=@orgId {andWhere}
					) AS [WorkItemCount]
				FROM
					[dbo].[Label] [l]
				WHERE
					[l].[OrganizationId]=@orgId
			) SELECT * FROM [source] WHERE [WorkItemCount]>0
			ORDER BY [WorkItemCount] DESC")
		{
		}

		public int OrgId { get; set; }

		[Where("[wi].[ApplicationId]=@appId")]
		public int? AppId { get; set; }
	}
}