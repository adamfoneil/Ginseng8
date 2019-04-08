using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class AppMilestones : Query<AppMilestone>
	{
		public AppMilestones() : base(
			@"SELECT [am].*, [app].[Name] AS [ApplicationName]
			FROM [dbo].[AppMilestone] [am]
			INNER JOIN [dbo].[Milestone] [ms] ON [am].[MilestoneId]=[ms].[Id]
			INNER JOIN [dbo].[Application] [app] ON [am].[ApplicationId]=[app].[Id]
			WHERE [app].[OrganizationId]=@orgId {andWhere}
			ORDER BY [ms].[Date]")
		{
		}

		public int OrgId { get; set; }

		[Where("[am].[ApplicationId]=@appId")]
		public int? AppId { get; set; }
	}
}