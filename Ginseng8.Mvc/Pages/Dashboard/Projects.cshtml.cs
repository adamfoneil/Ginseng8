using Ginseng.Models;
using Ginseng.Mvc.Classes;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Dashboard
{
	public enum ProjectViewOptions
	{
		Cards,
		Crosstab
	}

	[Authorize]
	public class ProjectsModel : DashboardPageModel, IActive
	{        
        private readonly FreshdeskCompanyCache _companyCache;

		public ProjectsModel(
            IConfiguration config,            
            FreshdeskCompanyCache companyCache) : base(config)
		{
			ShowExcelDownload = false; // because there are too many different options on this page for a single download, IMO            
            _companyCache = companyCache;
		}

		protected override Func<ClosedWorkItemsResult, int> ClosedItemGrouping => (ci) => ci.ProjectId ?? 0;

		public Dictionary<int, Project> Projects { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? Id { get; set; }

		[BindProperty(SupportsGet = true)]
		public ProjectViewOptions View { get; set; } = ProjectViewOptions.Cards;

		[BindProperty(SupportsGet = true)]
		public ProjectInfoSortOptions Sort { get; set; }

		[BindProperty(SupportsGet = true)]
		public ProjectInfoShowOptions Show { get; set; }

		[BindProperty(SupportsGet = true)]
		public bool IsActive { get; set; } = true;

		public int? CurrentAppId { get; set; }

		// crosstab rows (projects and completion status)
		public IEnumerable<ProjectInfoResult> ProjectInfo { get; set; }		

		// crosstab columns (milestone dates and related names)
		public ILookup<DateTime, Milestone> MilestoneDates { get; set; }

		// crosstab cells (assignments and labels)
		public ILookup<ProjectDashboardCell, OpenWorkItemsResult> ProjectWorkItems { get; set; }
		public ILookup<int, Label> Labels { get; set; }

		public Project SelectedProject { get; set; }
		public IEnumerable<Comment> ProjectComments { get; set; }

		// used when single project is displayed, contains all the calculations/metadata not present in the base record
		public ProjectInfoResult SelectedProjectInfo { get; set; }

        public SelectList FreshdeskCompanySelect { get; set; }

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
			int maxCols = 12 - CrosstabRowHeadingGridCols();			
			int milestoneCount = MilestoneDates.Count;
			int result = maxCols / milestoneCount;
			if ((maxCols % milestoneCount) == 1) result++;
			return result;
		}

		protected override OpenWorkItems GetQuery()
		{
			// we show details on only one project at a time
			if (Id.HasValue)
			{
				return new OpenWorkItems(QueryTraces)
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
				SelectedProjectInfo = await new ProjectInfo() { Id = Id, OrgId = OrgId }.ExecuteSingleAsync(connection);
				ProjectComments = await new Comments() { OrgId = OrgId, ObjectType = ObjectType.Project, ObjectIds = new[] { SelectedProject.Id } }.ExecuteAsync(connection);

                if (CurrentOrg.UseFreshdesk())
                {
                    var companies = await _companyCache.QueryAsync(CurrentOrg.Name);
                    FreshdeskCompanySelect = new SelectList(companies.Select(c => new SelectListItem() { Value = c.Id.ToString(), Text = c.Name }), "Value", "Text", SelectedProject.FreshdeskCompanyId);
                }                
            }
            else
			{
				// crosstab rows (or card view)
				ProjectInfo = await new ProjectInfo(Sort) { OrgId = OrgId, IsActive = IsActive, AppId = CurrentOrgUser.CurrentAppId, Show = Show }.ExecuteAsync(connection);
				if (!ProjectInfo.Any()) ProjectInfo = new ProjectInfoResult[] { new ProjectInfoResult() { Name = "Items Without a Project", ApplicationId = CurrentOrgUser.CurrentAppId ?? 0 } };

				if (View == ProjectViewOptions.Crosstab)
				{
					// crosstab columns
					var milestones = await new Milestones() { OrgId = OrgId, WithProjectsForAppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection);

					// crosstab cells
					var workItems = await new OpenWorkItems() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId }.ExecuteAsync(connection);
					ProjectWorkItems = workItems.ToLookup(row => new ProjectDashboardCell(row.ProjectId, row.MilestoneDate ?? DateTime.MaxValue));

					var workItemIds = workItems.Select(wi => wi.Id).ToArray();
					var labels = await new LabelsInUse() { OrgId = OrgId, WorkItemIds = workItemIds }.ExecuteAsync(connection);
					Labels = labels.ToLookup(row => row.WorkItemId);

					// there's only enough horizontal room for 3 milestones + optional placeholder for work items without a milestone
					var milestoneList = milestones.Take(3).ToList();
					// if there's any work item info without a milestone, then we need to append an empty milestone column to the crosstab
					if (workItems.Any(lbl => !lbl.MilestoneDate.HasValue))
					{
						milestoneList.Add(new Milestone() { Name = "Drag to a milestone", Date = DateTime.MaxValue, ShowDate = false });
					}

					MilestoneDates = milestoneList.ToLookup(row => row.Date);
				}
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
			await Data.TryDeleteAsync<Project>(id);
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