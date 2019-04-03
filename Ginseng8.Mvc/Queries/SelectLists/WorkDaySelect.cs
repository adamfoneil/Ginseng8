using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class WorkDaySelect : SelectListQuery
	{
		public WorkDaySelect() : base(
			@"SELECT [Value], [Name] AS [Text]
			FROM [app].[WorkDay]
			ORDER BY [Value]")
		{
		}
	}
}