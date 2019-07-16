using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
	public class Milestones : Query<Milestone>, ITestableQuery
	{
		public Milestones() : base(
            @"SELECT 
				[ms].*,
				DATEDIFF(d, getdate(), [ms].[Date]) AS [DaysAway],
                (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id]) AS [TotalWorkItems],
                (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL) AS [OpenWorkItems],
                (SELECT COUNT(1) FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NOT NULL) AS [ClosedWorkItems]
			FROM 
				[dbo].[Milestone] [ms]                
            WHERE
                [ms].[OrganizationId]=@orgId                 
                {andWhere}            
			ORDER BY 
				[Date]")
		{
		}
        
        public int OrgId { get; set; }

        [Where("[ms].[TeamId]=@teamId")]
        public int? TeamId { get; set; }

		[Where("[ms].[Date]>=@minDate")]
		public DateTime? MinDate { get; set; } = DateTime.Today.AddDays(-5);

        [Case(true, @"(
                    [ms].[Date]>getdate() OR 
                    ([ms].[Date]<getdate() AND EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL))
                )")]
        public bool? IsSelectable { get; set; } = true;

        [Where("EXISTS(SELECT 1 FROM [dbo].[WorkItem] [wi] INNER JOIN [dbo].[Application] [app] ON [wi].[ApplicationId]=[app].[Id] WHERE [wi].[ProjectId]=@projectId AND [wi].[MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL AND [app].[IsActive]=1)")]
        public int? ProjectId { get; set; }

		[Case(false, "NOT EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id])")]
		[Case(true, "EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id])")]
		public bool? HasWorkItems { get; set; }

		[Case(false, "NOT EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL)")]
		[Case(true, "EXISTS(SELECT 1 FROM [dbo].[WorkItem] WHERE [MilestoneId]=[ms].[Id] AND [CloseReasonId] IS NULL)")]
		public bool? HasOpenWorkItems { get; set; }

        [Where("[ms].[Id]=@id")]
        public int? Id { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {            
            yield return new Milestones() { OrgId = 0 };
            yield return new Milestones() { OrgId = 0, IsSelectable = true };
            yield return new Milestones() { OrgId = 0, TeamId = 0 };
            yield return new Milestones() { OrgId = 1, ProjectId = 1 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}