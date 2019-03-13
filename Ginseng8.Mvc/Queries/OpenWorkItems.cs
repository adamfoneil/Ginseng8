using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
	public class OpenWorkItemsResult
	{
		public int Id { get; set; }
		public int Number { get; set; }
		public string Title { get; set; }	
		public string HtmlBody { get; set; }
		public int? BusinessUserId { get; set; }
		public int? DeveloperUserId { get; set; }
		public int? BusinessPriority { get; set; }
		public int ApplicationId { get; set; }
		public string ApplicationName { get; set; }
		public int ProjectId { get; set; }
		public string ProjectName { get; set; }
		public int MilestoneId { get; set; }
		public string MilestoneName { get; set; }
		public DateTime? MilestoneDate { get; set; }
		public int? MilestoneDaysAway { get; set; }
		public int? CloseReasonId { get; set; }
		public string CloseReasonName { get; set; }
		public int ActivityId { get; set; }
		public string ActivityName { get; set; }
		public string BusinessName { get; set; }
		public string DeveloperName { get; set; }
		public int? AssignedUserId { get; set; }
		public string AssignedUserName { get; set; }		
		public string WorkItemSize { get; set; }
		public int? DevelopmentPriority { get; set; }
		public int? SizeId { get; set; }
		public int? DevEstimateHours { get; set; }
		public int? SizeEstimateHours { get; set; }
		public int EstimateHours { get; set; }
		public string WorkItemUserIdColumn { get; set; }
	}

	public class OpenWorkItems : Query<OpenWorkItemsResult>, ITestableQuery
	{
		public OpenWorkItems() : base(
			@"SELECT
                [wi].[Id],
                [wi].[Number],				
                [wi].[Title],
				[wi].[HtmlBody],
                [wi].[BusinessUserId],
                [wi].[DeveloperUserId],
                [wi].[Priority] AS [BusinessPriority],
                [wi].[ApplicationId], [app].[Name] AS [ApplicationName],
                COALESCE([wi].[ProjectId], 0) AS [ProjectId], COALESCE([p].[Name], '(no project)') AS [ProjectName],
                COALESCE([wi].[MilestoneId], 0) AS [MilestoneId], COALESCE([ms].[Name], '(no milestone)') AS [MilestoneName], COALESCE([ms].[Date], '12/31/9999') AS [MilestoneDate], DATEDIFF(d, getdate(), [ms].[Date]) AS [MilestoneDaysAway],
                [wi].[CloseReasonId], [cr].[Name] AS [CloseReasonName],
                COALESCE([wi].[ActivityId], 0) AS [ActivityId],
                [act].[Name] AS [ActivityName],
                COALESCE([biz_ou].[DisplayName], [ousr].[UserName]) AS [BusinessName],
                COALESCE([dev_ou].[DisplayName], [dusr].[UserName]) AS [DeveloperName],
                CASE [act].[ResponsibilityId]
                    WHEN 1 THEN COALESCE([biz_ou].[DisplayName], [ousr].[UserName])
                    WHEN 2 THEN COALESCE([dev_ou].[DisplayName], [dusr].[UserName])
                END AS [AssignedUserName],
				CASE [act].[ResponsibilityId]
					WHEN 1 THEN [wi].[BusinessUserId]
					WHEN 2 THEN [wi].[DeveloperUserId]
				END AS [AssignedUserId],
                [sz].[Name] AS [WorkItemSize],                
                [wid].[Priority] AS [DevelopmentPriority],
                [wi].[SizeId],
                [wid].[EstimateHours] AS [DevEstimateHours],
                [sz].[EstimateHours] AS [SizeEstimateHours],
				COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0) AS [EstimateHours],
				[r].[WorkItemUserIdColumn]
            FROM
                [dbo].[WorkItem] [wi]
                INNER JOIN [dbo].[Application] [app] ON [wi].[ApplicationId]=[app].[Id]
                LEFT JOIN [dbo].[Project] [p] ON [wi].[ProjectId]=[p].[Id]
                LEFT JOIN [dbo].[Activity] [act] ON [wi].[ActivityId]=[act].[Id]
                LEFT JOIN [app].[Responsibility] [r] ON [act].[ResponsibilityId]=[r].[Id]
                LEFT JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
                LEFT JOIN [app].[CloseReason] [cr] ON [wi].[CloseReasonId]=[cr].[Id]
                LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
                LEFT JOIN [dbo].[OrganizationUser] [biz_ou] ON
                    [wi].[OrganizationId]=[biz_ou].[OrganizationId] AND
                    [wi].[BusinessUserId]=[biz_ou].[UserId]
                LEFT JOIN [dbo].[AspNetUsers] [ousr] ON [wi].[BusinessUserId]=[ousr].[UserId]
                LEFT JOIN [dbo].[OrganizationUser] [dev_ou] ON
                    [wi].[OrganizationId]=[dev_ou].[OrganizationId] AND
                    [wi].[DeveloperUserId]=[dev_ou].[UserId]
                LEFT JOIN [dbo].[AspNetUsers] [dusr] ON [wi].[DeveloperUserId]=[dusr].[UserId]
                LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
            WHERE
                [wi].[OrganizationId]=@orgId AND [wi].[CloseReasonId] IS NULL {andWhere}
            ORDER BY
                [p].[Priority],
                COALESCE([wid].[Priority], [wi].[Priority]), 
                [wi].[Number]")
		{
		}

		public int OrgId { get; set; }

		[Where("[wi].[Number]=@number")]
		public int? Number { get; set; }

		[Where("(CASE [act].[ResponsibilityId] WHEN 1 THEN [wi].[BusinessUserId] WHEN 2 THEN [wi].[DeveloperUserId] END)=@assignedUserId")]
		public int? AssignedUserId { get; set; }

		[Where("[wi].[ApplicationId]=@appId")]
		public int? AppId { get; set; }

		[Case(true, "[wi].[MilestoneId] IS NOT NULL")]
		public bool? HasMilestone { get; set; }

		[Where("[wi].[ProjectId]=@projectId")]
		public int? ProjectId { get; set; }

		[Where("EXISTS(SELECT 1 FROM [dbo].[WorkItemLabel] WHERE [WorkItemId]=[wi].[Id] AND [LabelId]=@labelId)")]
		public int? LabelId { get; set; }

		public static IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new OpenWorkItems() { OrgId = 0 };
		}

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}
	}
}