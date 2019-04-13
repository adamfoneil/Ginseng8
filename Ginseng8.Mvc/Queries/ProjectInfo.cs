using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

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
		public int? EstimateHours { get; set; }
		public bool HasImpediment { get; set; }
	}

	public enum ProjectInfoSortOptions
	{
		Priority,
		Name,

		[Description("Open work items")]
		OpenWorkItems,

		[Description("Total work items")]
		TotalWorkItems,

		[Description("Percent complete")]
		PercentComplete,

		[Description("Estimate hours")]
		EstimateHours
	}

	public enum ProjectInfoShowOptions
	{
		All,

		[Description("Has open items")]
		HasOpenItems,

		[Description("Has no items")]
		NoOpenItems
	}

	public class ProjectInfo : Query<ProjectInfoResult>, ITestableQuery
	{
		public ProjectInfo(ProjectInfoSortOptions sort = ProjectInfoSortOptions.Priority) : base(
			$@"WITH [source] AS (
				SELECT
					[p].*,
					[app].[Name] AS [ApplicationName],
					(SELECT 
						SUM(COALESCE([wid].[EstimateHours], [sz].[EstimateHours])) 
						FROM [dbo].[WorkItem] [wi] LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id] 
						LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
						WHERE [wi].[ProjectId]=[p].[Id]) AS [EstimateHours],
					(SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id]) AS [TotalWorkItems],
					(SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id] AND [CloseReasonId] IS NULL) AS [OpenWorkItems],
					(SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id] AND [CloseReasonId] IS NOT NULL) AS [ClosedWorkItems],
					CASE
						WHEN EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id]) THEN 0
						WHEN [p].[HtmlBody] IS NOT NULL THEN 0
						ELSE 1
					END AS [AllowDelete],
					CASE
						WHEN EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [HasImpediment]=1 AND [ProjectId]=[p].[Id]) THEN 1
						ELSE 0
					END AS [HasImpediment]
				FROM
					[dbo].[Project] [p]
					INNER JOIN [dbo].[Application] [app] ON [p].[ApplicationId]=[app].[Id]
				WHERE
					[app].[OrganizationId]=@orgId 					
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

		[Where("[p].[IsActive]=@isActive")]
		public bool? IsActive { get; set; }

		[Case(ProjectInfoShowOptions.HasOpenItems, "EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id] AND [CloseReasonId] IS NULL)")]
		[Case(ProjectInfoShowOptions.NoOpenItems, "NOT EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [ProjectId]=[p].[Id] AND [CloseReasonId] IS NULL)")]
		public ProjectInfoShowOptions Show { get; set; } = ProjectInfoShowOptions.All;

		[Phrase("p].[Name", "p].[TextBody")]
		public string TitleAndBodySearch { get; set; }

		[Where("[p].[Id]=@id")]
		public int? Id { get; set; }

		private static Dictionary<ProjectInfoSortOptions, string> SortOptions
		{
			get
			{
				return new Dictionary<ProjectInfoSortOptions, string>()
				{
					{ ProjectInfoSortOptions.Priority, "[Priority] ASC, [Name] ASC" },
					{ ProjectInfoSortOptions.Name, "[Name] ASC" },
					{ ProjectInfoSortOptions.OpenWorkItems, "[OpenWorkItems] DESC" },
					{ ProjectInfoSortOptions.TotalWorkItems, "[TotalWorkItems] DESC" },
					{ ProjectInfoSortOptions.PercentComplete, "[PercentComplete] DESC" },
					{ ProjectInfoSortOptions.EstimateHours, "[EstimateHours] DESC" }
				};
			}
		}

		public static IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new ProjectInfo() { OrgId = 0 };
			yield return new ProjectInfo(ProjectInfoSortOptions.Name) { OrgId = 0 };
			yield return new ProjectInfo(ProjectInfoSortOptions.Priority) { OrgId = 0 };
			yield return new ProjectInfo(ProjectInfoSortOptions.OpenWorkItems) { OrgId = 0 };
			yield return new ProjectInfo(ProjectInfoSortOptions.TotalWorkItems) { OrgId = 0 };
			yield return new ProjectInfo(ProjectInfoSortOptions.PercentComplete) { OrgId = 0 };
			yield return new ProjectInfo(ProjectInfoSortOptions.EstimateHours) { OrgId = 0 };
		}

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}
	}
}