using Postulate.Base;
using Postulate.Base.Attributes;
using System;
using System.Collections.Generic;

namespace Ginseng.Mvc.Queries
{
	public class ProjectInfoResult
	{
		public int Id { get; set; }
		public int ApplicationId { get; set; }
		public string ApplicationName { get; set; }
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
		public float PercentComplete { get; set; }
		public bool AllowDelete { get; set; }
	}

	public enum ProjectInfoSortOptions
	{
		Name,
		OpenWorkItems,
		TotalWorkItems,
		PercentComplete
	}

	public enum ProjectInfoShowOptions
	{
		All,
		HasOpenItems,
		NoOpenItems
	}

	public class ProjectInfo : Query<ProjectInfoResult>
	{
		public ProjectInfo(ProjectInfoSortOptions sort = ProjectInfoSortOptions.Name) : base(
			$@"WITH [source] AS (
				SELECT
					[p].*,
					[app].[Name] AS [ApplicationName],
					(SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id]) AS [TotalWorkItems],
					(SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id] AND [CloseReasonId] IS NULL) AS [OpenWorkItems],
					(SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id] AND [CloseReasonId] IS NOT NULL) AS [ClosedWorkItems],
					CASE
						WHEN EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id]) THEN 0
						WHEN [p].[HtmlBody] IS NOT NULL THEN 0
						ELSE 1
					END AS [AllowDelete]
				FROM
					[dbo].[Project] [p]
					INNER JOIN [dbo].[Application] [app] ON [p].[ApplicationId]=[app].[Id]
				WHERE
					[app].[OrganizationId]=@orgId AND
					[p].[IsActive]=@isActive
					{{andWhere}}
			) SELECT
				[source].*,
				CASE
					WHEN [TotalWorkItems] > 0 THEN CONVERT(float, [ClosedWorkItems]) / CONVERT(float, [TotalWorkItems])
					ELSE 0
				END AS [PercentComplete]
			FROM
				[source]
			ORDER BY {SortOptions[sort]}")
		{
		}

		public int OrgId { get; set; }

		[Where("[p].[ApplicationId]=@appId")]
		public int? AppId { get; set; }

		[Case(ProjectInfoShowOptions.HasOpenItems, "EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id] AND [CloseReasonId] IS NULL)")]
		[Case(ProjectInfoShowOptions.NoOpenItems, "NOT EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id] AND [CloseReasonId] IS NULL)")]
		public ProjectInfoShowOptions Show { get; set; } = ProjectInfoShowOptions.All;

		[Phrase("p].[Name", "p].[TextBody")]
		public string TitleAndBodySearch { get; set; }

		public bool IsActive { get; set; }

		private static Dictionary<ProjectInfoSortOptions, string> SortOptions
		{
			get
			{
				return new Dictionary<ProjectInfoSortOptions, string>()
				{
					{ ProjectInfoSortOptions.Name, "[Name] ASC" },
					{ ProjectInfoSortOptions.OpenWorkItems, "[OpenWorkItems] DESC" },
					{ ProjectInfoSortOptions.TotalWorkItems, "[TotalWorkItems] DESC" },
					{ ProjectInfoSortOptions.PercentComplete, "[PercentComplete] DESC" }
				};
			}
		}
	}
}