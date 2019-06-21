using Ginseng.Mvc.Classes;
using Postulate.Base.Attributes;

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
				[Date]>DATEADD(d, -7, getdate()) {andWhere}
			ORDER BY
				[Date]")
		{
		}

        [Where("[ApplicationId]=@appId")]
		public int? AppId { get; set; }

        [Where("[TeamId]=@appId")]
        public int? TeamId { get; set; }
	}
}