using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class SizeSelect : SelectListQuery
	{
		public SizeSelect() : base(
			@"SELECT [Id] AS [Value], [Name] AS [Text]
			FROM [dbo].[WorkItemSize]
			WHERE [OrganizationId]=@orgId
			ORDER BY [EstimateHours]")
		{
		}

		public int OrgId { get; set; }
	}
}