using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
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
	public class ProjectsModel : DashboardPageModel, IActive
	{
		public ProjectsModel(IConfiguration config) : base(config)
		{
		}

		public Dictionary<int, Project> Projects { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? Id { get; set; }

		[BindProperty(SupportsGet = true)]
		public ProjectInfoSortOptions Sort { get; set; }

		[BindProperty(SupportsGet = true)]
		public bool IsActive { get; set; } = true;

		public int? CurrentAppId { get; set; }

		public IEnumerable<ProjectInfoResult> ProjectInfo { get; set; }
		public ILookup<int, ProjectInfoLabelsResult> ProjectLabels { get; set; }
		public ILookup<int, ProjectInfoAssignmentsResult> ProjectAssignments { get; set; }

		public Project SelectedProject { get; set; }

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
				SelectedProject = await Data.FindAsync<Project>(Id.Value);
			}
			else
			{
				ProjectInfo = await new ProjectInfo(Sort) { OrgId = OrgId, IsActive = IsActive, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection);
				if (!ProjectInfo.Any()) ProjectInfo = new ProjectInfoResult[] { new ProjectInfoResult() { ApplicationId = CurrentOrgUser.CurrentAppId ?? 0 } };

				var labels = await new ProjectInfoLabels() { OrgId = OrgId }.ExecuteAsync(connection);
				ProjectLabels = labels.ToLookup(row => row.ProjectId);

				var assignments = await new ProjectInfoAssignments() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection);
				ProjectAssignments = assignments.ToLookup(row => row.ProjectId);
			}
		}

		public async Task<IActionResult> OnPostCreate(int applicationId, string name)
		{
			var project = new Project() { Name = name, ApplicationId = applicationId };			
			await Data.TrySaveAsync(project);
			return Redirect($"Projects/{project.Id}");
		}

		public async Task<IActionResult> OnPostDelete(int id)
		{
			await Data.TryDelete<Project>(id);
			return Redirect("Projects");
		}

		public async Task<IActionResult> OnGetDeactivate(int id)
		{
			return await SetProjectActive(id, false);
		}

		public async Task<IActionResult> OnGetActivate(int id)
		{
			return await SetProjectActive(id, true);
		}

		private async Task<IActionResult> SetProjectActive(int id, bool isActive)
		{
			var project = await Data.FindAsync<Project>(id);
			project.IsActive = isActive;
			await Data.TryUpdateAsync(project, r => r.IsActive);
			return Redirect("Projects");
		}
	}
}