using Postulate.Base;
using System;

namespace Ginseng.Mvc.Queries
{
	public class ProjectInfoLabelsResult
	{
		public int ProjectId { get; set; }
		public string LabelName { get; set; }
		public string BackColor { get; set; }
		public string ForeColor { get; set; }
		public int Count { get; set; }
		public DateTime? MilestoneDate { get; set; }

		public DateTime DateValue()
		{
			return MilestoneDate ?? DateTime.MaxValue;
		}
	}

	public class ProjectInfoLabels : Query<ProjectInfoLabelsResult>
	{
		public ProjectInfoLabels() : base(
			@"SELECT
				[p].[Id] AS [ProjectId],
				[lbl].[Name] AS [LabelName],
				[lbl].[BackColor] AS [BackColor],
				[lbl].[ForeColor] AS [ForeColor],
				COUNT(1) AS [Count],
				[ms].[Date] AS [MilestoneDate]
			FROM
				[dbo].[Project] [p]
				INNER JOIN [dbo].[WorkItem] [wi] ON [p].[Id]=[wi].[ProjectId]
				INNER JOIN [dbo].[WorkItemLabel] [wil] ON [wi].[Id]=[wil].[WorkItemId]
				INNER JOIN [dbo].[Label] [lbl] ON [wil].[LabelId]=[lbl].[Id]			
				LEFT JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
			WHERE
				[lbl].[OrganizationId]=@orgId
			GROUP BY
				[p].[Id],
				[lbl].[Name],
				[lbl].[BackColor],
				[lbl].[ForeColor]
			ORDER BY
				COUNT(1) DESC")
		{
		}

		public int OrgId { get; set; }
	}
}