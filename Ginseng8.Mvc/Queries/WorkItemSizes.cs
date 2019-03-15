using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class WorkItemSizes : Query<WorkItemSize>
	{
		public WorkItemSizes() : base(
			@"WITH [source] AS (
				SELECT 
					[wis].*,		
					ROW_NUMBER() OVER (ORDER BY [EstimateHours]) - 1 AS [RowNumber]
				FROM 
					[dbo].[WorkItemSize] [wis]
				WHERE 
					[OrganizationId]=@orgId
			), [maxRow] AS (
				SELECT MAX([RowNumber]) AS [RowCount] FROM [source]
			) SELECT
				*,
				CONVERT(float, [RowNumber]) / CONVERT(float, [RowCount]) AS [ColorGradientPosition]
			FROM
				[source],
				[maxRow]	
			ORDER BY
				[EstimateHours]")
		{
		}

		public int OrgId { get; set; }
	}
}