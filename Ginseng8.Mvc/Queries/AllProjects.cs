using Microsoft.AspNetCore.Mvc.Rendering;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class AllProjectsResult
	{
		public int ApplicationId { get; set; }
		public int Value { get; set; }
		public string Text { get; set; }

		public SelectListItem ToSelectListItem()
		{
			return new SelectListItem() { Value = Value.ToString(), Text = Text };
		}
	}

	public class AllProjects : Query<AllProjectsResult>
	{
		public AllProjects() : base(
			@"SELECT
				[p].[ApplicationId], [p].[Id] AS [Value], [p].[Name] AS [Text]
			FROM
				[dbo].[Project] [p]
				INNER JOIN [dbo].[Application] [app] ON [p].[ApplicationId]=[app].[Id]
			WHERE
				[app].[OrganizationId]=@orgId AND
				[p].[IsActive]=1
			ORDER BY
				[p].[ApplicationId], [p].[Name]")
		{
		}

		public int OrgId { get; set; }
	}
}