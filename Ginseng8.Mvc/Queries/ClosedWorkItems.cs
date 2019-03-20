﻿using Postulate.Base;
using Postulate.Base.Attributes;
using System;

namespace Ginseng.Mvc.Queries
{
	public class ClosedWorkItemsResult
	{
		public int? CloseReasonId { get; set; }
		public string CloseReasonName { get; set; }
		public int ApplicationId { get; set; }
		public int? MilestoneId { get; set; }
		public int Number { get; set; }
		public string Title { get; set; }
		public string ProjectName { get; set; }
		public int? DeveloperUserId { get; set; }
		public int? BusinessUserId { get; set; }
		public DateTime? DateModified { get; set; }
	}

	public class ClosedWorkItems : Query<ClosedWorkItemsResult>
	{
		public ClosedWorkItems() : base(
			@"SELECT
				[wi].[CloseReasonId],
				[cr].[Name] AS [CloseReasonName],
				[wi].[ApplicationId],
				[wi].[MilestoneId],
				[wi].[Number],
				[wi].[Title],
				[p].[Name] AS [ProjectName],
				[wi].[DeveloperUserId],
				[wi].[BusinessUserId],
				[wi].[DateModified]
			FROM
				[dbo].[WorkItem] [wi]
				INNER JOIN [app].[CloseReason] [cr] ON [wi].[CloseReasonId]=[cr].[Id]
				LEFT JOIN [dbo].[Project] [p] ON [wi].[ProjectId]=[p].[Id]
			WHERE
				[wi].[OrganizationId]=@orgId
				{andWhere}
			ORDER BY
				[wi].[DateModified] DESC")
		{
		}

		public int OrgId { get; set; }

		[Where("[wi].[ApplicationId]=@appId")]
		public int? AppId { get; set; }
	}
}