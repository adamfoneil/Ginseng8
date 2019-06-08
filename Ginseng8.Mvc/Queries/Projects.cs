using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class Projects : Query<Project>
	{
		public Projects() : base("SELECT [p].* FROM [dbo].[Project] [p] WHERE [IsActive]=@isActive {andWhere} ORDER BY [Priority], [Name]")
		{
		}

		[Where("[ApplicationId]=@appId")]
		public int? AppId { get; set; }

		public bool IsActive { get; set; }

		[Where("[Id] IN @includeIds")]
		public int[] IncludeIds { get; set; }

		[Where("[DataModelId]=@dataModelId")]
		public int? DataModelId { get; set; }

        [Case(true, "EXISTS(SELECT 1 FROM [dbo].[WorkItem] [wi] INNER JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id] WHERE [ms].[Date]>getdate() AND [wi].[ApplicationId]=@appId AND [wi].[ProjectId]=[p].[Id])")]
        public bool? HasMilestones { get; set; }
	}
}