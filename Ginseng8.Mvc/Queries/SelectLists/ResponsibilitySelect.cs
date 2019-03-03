using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class ResponsibilitySelect : SelectListQuery
	{
		public ResponsibilitySelect() : base(
			@"SELECT [Id] AS [Value], [Name] AS [Text]
			FROM [app].[Responsibility]
			ORDER BY [Name]")
		{
		}
	}
}