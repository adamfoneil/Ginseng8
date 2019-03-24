using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class ProjectSelect : SelectListQuery
	{
		public ProjectSelect() : base(
			@"SELECT [p].[Id] AS [Value], [p].[Name] AS [Text]
			FROM [dbo].[Project] [p]			
			WHERE [p].[ApplicationId]=@appId AND [p].[IsActive]=1
			ORDER BY [p].[Name]")
		{
		}

		public int AppId { get; set; }
	}
}