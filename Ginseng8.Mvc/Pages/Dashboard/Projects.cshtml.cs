using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Work
{
	[Authorize]
	public class ProjectsModel : DashboardPageModel
	{
		public ProjectsModel(IConfiguration config) : base(config)
		{
		}

		public Dictionary<int, Project> Projects { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? Id { get; set; }

		[BindProperty(SupportsGet = true)]
		public ProjectInfoSortOptions ProjectListSort { get; set; }

		public int? CurrentAppId { get; set; }

		public IEnumerable<ProjectInfoResult> ProjectInfo { get; set; }
		public ILookup<int, ProjectInfoLabelsResult> ProjectLabels { get; set; }
		public ILookup<int, ProjectInfoAssignmentsResult> ProjectAssignments { get; set; }

		protected override OpenWorkItems GetQuery()
		{
			// we show details on only one project at a time
			if (Id.HasValue)
			{
				return new OpenWorkItems()
				{
					OrgId = OrgId,					
					ProjectId = Id,
					AppId = CurrentOrgUser.CurrentAppId,
					LabelId = LabelId
				};
			}

			return null;
		}

		protected override async Task OnGetInternalAsync(SqlConnection connection)
		{
			CurrentAppId = CurrentOrgUser.CurrentAppId;

			if (Id.HasValue)
			{
				int[] projectIds = WorkItems
					.GroupBy(row => row.ProjectId)
					.Select(grp => grp.Key).ToArray();

				var projects = await new Projects() { OrgId = OrgId, IsActive = true, IncludeIds = projectIds }.ExecuteAsync(connection);
				Projects = projects.ToDictionary(row => row.Id);
			}
			else
			{
				ProjectInfo = await new ProjectInfo(ProjectListSort) { OrgId = OrgId }.ExecuteAsync(connection);

				var labels = await new ProjectInfoLabels() { OrgId = OrgId }.ExecuteAsync(connection);
				ProjectLabels = labels.ToLookup(row => row.ProjectId);

				var assignments = await new ProjectInfoAssignments() { OrgId = OrgId }.ExecuteAsync(connection);
				ProjectAssignments = assignments.ToLookup(row => row.ProjectId);
			}
		}
	}
}