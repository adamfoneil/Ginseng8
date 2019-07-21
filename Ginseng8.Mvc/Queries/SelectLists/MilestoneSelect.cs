using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class MilestoneSelect : SelectListQuery
	{
		public MilestoneSelect() : base(
			@"SELECT 
				[ms].[Id] AS [Value], 
				(CASE 
                    WHEN [ms].[TeamId] IS NULL THEN [ms].[Name]
                    WHEN [ms].[TeamId] IS NOT NULL THEN [t].[Name] + ': ' + [ms].[Name]
                END) + ': ' + FORMAT([Date], 'ddd M/d') AS [Text]
			FROM 
				[dbo].[Milestone] [ms]
                LEFT JOIN [dbo].[Team] [t] ON [ms].[TeamId]=[t].[Id]
			WHERE 
				[ms].[OrganizationId]=@orgId AND
				[Date]>DATEADD(d, -7, getdate())
			ORDER BY
				[Date]")
		{
		}

		public int OrgId { get; set; }
	}
}