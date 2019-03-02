using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class ProjectSelect : SelectListQuery
	{
		public ProjectSelect() : base(
			@"SELECT [Id] AS [Value], [Name] AS [Text]
			FROM [dbo].[Project]
			WHERE [OrganizationId]=@orgId AND [IsActive]=1
			ORDER BY [Priority], [Name]")
		{
		}

		public int OrgId { get; set; }
	}
}