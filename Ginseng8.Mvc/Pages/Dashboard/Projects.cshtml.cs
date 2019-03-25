using Ginseng.Models;
using Ginseng.Mvc.Classes;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
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

		protected override Func<ClosedWorkItemsResult, int> ClosedItemGrouping => (ci) => ci.ProjectId ?? 0;

		public Dictionary<int, Project> Projects { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? Id { get; set; }

		[BindProperty(SupportsGet = true)]
		public ProjectInfoSortOptions Sort { get; set; }

		[BindProperty(SupportsGet = true)]
		public bool IsActive { get; set; } = true;

		public int? CurrentAppId { get; set; }

		// crosstab rows (projects and completion status)
		public IEnumerable<ProjectInfoResult> ProjectInfo { get; set; }		

		// crosstab columns (milestone dates and related names)
		public ILookup<DateTime, Milestone> MilestoneDates { get; set; }

		// crosstab cells (assignments and labels)
		public ILookup<ProjectDashboardCell, ProjectInfoAssignmentsResult> ProjectAssignments { get; set; }
		public ILookup<ProjectDashboardCell, ProjectInfoLabelsResult> ProjectLabels { get; set; }

		public Project SelectedProject { get; set; }

		public int CrosstabRowHeadingGridCols()
		{
			// assuming up to 4 milestone columns, leaving a minimum of 4 for the row headings,
			// each milestone will take 2 grid columns (for a total of 8 grid cols)
			int result = 8 - MilestoneDates.Count;

			// if there's an odd number of milestone columns, then add 1 from the row headings
			if ((MilestoneDates.Count % 2) == 1) result++;

			// assume 3 milestones, gives us 5 row heading, leaving 7 for the milestones to divide up.
			// that doesn't divide evenly by 2, so we add one to the row headings

			// assume 2 milestones, gives us 6, this divides evenly by 2, so nothing to change
			// with 4 milestones, gives us 4, which divides evenly by 2, leaving no change

			return result;
		}

		public int CrosstabColumnHeadingGridCols()
		{
			// assuming max of 4 milestone columns
			return (MilestoneDates.Count / 4) * 2;
		}

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

			// no project is selected, so we don't display any work items
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
				// crosstab rows
				ProjectInfo = await new ProjectInfo(Sort) { OrgId = OrgId, IsActive = IsActive, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection);
				if (!ProjectInfo.Any()) ProjectInfo = new ProjectInfoResult[] { new ProjectInfoResult() { ApplicationId = CurrentOrgUser.CurrentAppId ?? 0 } };

				// crosstab columns
				var milestones = await new Milestones() { OrgId = OrgId, HasOpenWorkItems = true }.ExecuteAsync(connection);
				
				// crosstab cells
				var labels = await new ProjectInfoLabels() { OrgId = OrgId }.ExecuteAsync(connection);
				ProjectLabels = labels.ToLookup(row => new ProjectDashboardCell(row.ProjectId, row.DateValue()));

				var assignments = await new ProjectInfoAssignments() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection);
				ProjectAssignments = assignments.ToLookup(row => new ProjectDashboardCell(row.ProjectId, row.DateValue()));

				// there's only enough horizontal room for 3 milestones + optional placeholder for work items without a milestone
				var milestoneList = milestones.Take(3).ToList();
				// if there's any work item info without a milestone, then we need to append an empty milestone column to the crosstab
				if (labels.Any(lbl => !lbl.MilestoneDate.HasValue) || assignments.Any(a => !a.MilestoneDate.HasValue))
				{
					milestoneList.Add(new Milestone() { Name = "No Milestone", Date = DateTime.MaxValue, ShowDate = false });
				}
				
				MilestoneDates = milestoneList.ToLookup(row => row.Date);
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