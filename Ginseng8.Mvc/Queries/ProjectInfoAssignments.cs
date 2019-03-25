using Postulate.Base;
using Postulate.Base.Attributes;
using System;

namespace Ginseng.Mvc.Queries
{
	public class ProjectInfoAssignmentsResult
	{
		public int ProjectId { get; set; }
		public string AssignedUserName { get; set; }
		public int WorkItemCount { get; set; }
		public DateTime? MilestoneDate { get; set; }

		public DateTime DateValue()
		{
			return MilestoneDate ?? DateTime.MaxValue;
		}
	}

	public class ProjectInfoAssignments : Query<ProjectInfoAssignmentsResult>
	{
		public ProjectInfoAssignments() : base(
			@"WITH [source] AS (
				SELECT
					[wi].[ProjectId],
					[wi].[Id],
					CASE [act].[ResponsibilityId]
						WHEN 1 THEN [wi].[BusinessUserId]
						WHEN 2 THEN [wi].[DeveloperUserId]
					END AS [AssignedUserId],
					[ms].[Date] AS [MilestoneDate]
				FROM
					[dbo].[WorkItem] [wi]
					LEFT JOIN [dbo].[Activity] [act] ON [wi].[ActivityId]=[act].[Id]
					LEFT JOIN [app].[Responsibility] [r] ON [act].[ResponsibilityId]=[r].[Id]
					LEFT JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
				WHERE
					[wi].[OrganizationId]=@orgId AND					
					[wi].[ProjectId] IS NOT NULL {andWhere}
			) SELECT
				[ProjectId],
				COALESCE([ou].[DisplayName], [u].[UserName]) AS [AssignedUserName],
				COUNT(1) AS [WorkItemCount],
				[MilestoneDate]
			FROM
				[source]
				LEFT JOIN [dbo].[AspNetUsers] [u] ON [source].[AssignedUserId]=[u].[UserId]
				LEFT JOIN [dbo].[OrganizationUser] [ou] ON [source].[AssignedUserId]=[ou].[UserId] AND [ou].[OrganizationId]=@orgId
			GROUP BY
				[ProjectId],
				COALESCE([ou].[DisplayName], [u].[UserName]),
				[MilestoneDate]
			ORDER BY
				COUNT(1) DESC")
		{
		}

		public int OrgId { get; set; }

		[Where("[wi].[ApplicationId]=@appId")]
		public int? AppId { get; set; }
	}
}