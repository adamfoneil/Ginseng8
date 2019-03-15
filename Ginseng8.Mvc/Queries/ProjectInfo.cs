using Postulate.Base;
using System;
using System.Collections.Generic;

namespace Ginseng.Mvc.Queries
{
	public class ProjectInfoResult
	{
		public int Id { get; set; }
		public int OrganizationId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int? Priority { get; set; }
		public string BranchUrl { get; set; }
		public string TextBody { get; set; }
		public string HtmlBody { get; set; }
		public bool IsActive { get; set; }
		public string CreatedBy { get; set; }
		public DateTime DateCreated { get; set; }
		public string ModifiedBy { get; set; }
		public DateTime? DateModified { get; set; }
		public int? TotalWorkItems { get; set; }
		public int? OpenWorkItems { get; set; }
		public int? ClosedWorkItems { get; set; }
		public int? PercentComplete { get; set; }
		public bool AllowDelete { get; set; }
	}

	public enum ProjectInfoSortOptions
	{
		Name,
		OpenWorkItems,
		TotalWorkItems,
		PercentComplete
	}

	public class ProjectInfo : Query<ProjectInfoResult>
	{
		public ProjectInfo(ProjectInfoSortOptions sort = ProjectInfoSortOptions.Name) : base(
			$@"WITH [source] AS (
				SELECT
					[p].*,
					(SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id]) AS [TotalWorkItems],
					(SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id] AND [CloseReasonId] IS NULL) AS [OpenWorkItems],
					(SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id] AND [CloseReasonId] IS NOT NULL) AS [ClosedWorkItems],
					CASE
						WHEN EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id]) THEN 0
						ELSE 1
					END AS [AllowDelete]
				FROM
					[dbo].[Project] [p]
				WHERE
					[p].[OrganizationId]=@orgId					
			) SELECT
				[source].*,
				CASE
					WHEN [TotalWorkItems] > 0 THEN [ClosedWorkItems] / [TotalWorkItems]
					ELSE 0
				END AS [PercentComplete]
			FROM
				[source]
			ORDER BY {SortOptions[sort]}")
		{
		}

		public int OrgId { get; set; }

		private static Dictionary<ProjectInfoSortOptions, string> SortOptions
		{
			get
			{
				return new Dictionary<ProjectInfoSortOptions, string>()
				{
					{ ProjectInfoSortOptions.Name, "[Name] ASC" },
					{ ProjectInfoSortOptions.OpenWorkItems, "[OpenWorkItems] DESC" },
					{ ProjectInfoSortOptions.TotalWorkItems, "[TotalWorkItems] DESC" },
					{ ProjectInfoSortOptions.PercentComplete, "[PercentComplete] ASC" }
				};
			}
		}
	}
}