using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class CloseReasonSelect : SelectListQuery
	{
		public CloseReasonSelect() : base(
			@"SELECT [Id] AS [Value], [Name] AS [Text]
			FROM [app].[CloseReason]
			ORDER BY [Name]")
		{
		}
	}
}