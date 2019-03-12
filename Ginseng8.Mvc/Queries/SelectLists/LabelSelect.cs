using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class LabelSelect : SelectListQuery
	{
		public LabelSelect() : base(
			@"SELECT [Id] AS [Value], [Name] AS [Text]
			FROM [dbo].[Label]
			WHERE [OrganizationId]=@orgId AND [IsActive]=1
			ORDER BY [Name]")
		{
		}

		public int OrgId { get; set; }
	}
}