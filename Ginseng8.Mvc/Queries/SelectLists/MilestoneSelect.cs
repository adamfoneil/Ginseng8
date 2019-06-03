using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class MilestoneSelect : SelectListQuery
	{
		public MilestoneSelect() : base(
			@"SELECT 
				[Id] AS [Value], 
				[Name] + ': ' + FORMAT([Date], 'ddd M/d') AS [Text]
			FROM 
				[dbo].[Milestone]
			WHERE 
				[ApplicationId]=@appId AND
				[Date]>DATEADD(d, -7, getdate())
			ORDER BY
				[Date]")
		{
		}

		public int AppId { get; set; }
	}
}