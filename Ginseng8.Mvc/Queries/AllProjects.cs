using Microsoft.AspNetCore.Mvc.Rendering;
using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
	public class AllProjectsResult
	{
        public int TeamId { get; set; }
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
				[p].[ApplicationId], [p].[TeamId], [p].[Id] AS [Value], [p].[Name] AS [Text]
			FROM
				[dbo].[Project] [p]
				INNER JOIN [dbo].[Team] [t] ON [p].[TeamId]=[t].[Id]
			WHERE
				[t].[OrganizationId]=@orgId AND
				[p].[IsActive]=1
			ORDER BY
				[p].[Name]")
		{
		}

		public int OrgId { get; set; }
	}
}