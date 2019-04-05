using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class TimeZoneSelect : SelectListQuery
	{
		public TimeZoneSelect() : base(
			"SELECT [Offset] AS [Value], [Name] AS [Text] FROM [app].[TimeZone] ORDER BY [Name]")
		{
		}
	}
}