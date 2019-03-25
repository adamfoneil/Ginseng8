using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;
using System;

namespace Ginseng.Mvc.Queries
{
	public class Milestones : Query<Milestone>
	{
		public Milestones() : base(
			@"SELECT 
				[ms].*,
				DATEDIFF(d, getdate(), [ms].[Date]) AS [DaysAway]
			FROM 
				[dbo].[Milestone] [ms]
			WHERE 
				[OrganizationId]=@orgId				
				{andWhere}
			ORDER BY 
				[Date]")
		{
		}

		public int OrgId { get; set; }

		[Where("[ms].[Date]>=@minDate")]
		public DateTime? MinDate { get; set; } = DateTime.Today.AddDays(-5);

		[Case(false, "NOT EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id])")]
		[Case(true, "EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id])")]
		public bool? HasWorkItems { get; set; }

		[Case(false, "NOT EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL)")]
		[Case(true, "EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL)")]
		public bool? HasOpenWorkItems { get; set; }

		[Where("EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [ApplicationId]=@withProjectsForAppId AND [ProjectId] IS NOT NULL AND [CloseReasonId] IS NULL)")]
		public int? WithProjectsForAppId { get; set; }
	}
}