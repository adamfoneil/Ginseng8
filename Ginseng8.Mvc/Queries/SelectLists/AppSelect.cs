using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class AppSelect : SelectListQuery
	{
		public AppSelect() : base(
			@"SELECT [Id] AS [Value], [Name] AS [Text]
			FROM [dbo].[Application] [app]
			WHERE [OrganizationId]=@orgId AND [IsActive]=1
			ORDER BY [Name]")
		{
		}

		public int OrgId { get; set; }
	}
}