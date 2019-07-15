using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : DashboardPageModel
    {
        public IndexModel(IConfiguration config) : base(config)
        {
            ShowExcelDownload = false;
            ShowLabelFilter = false;
        }

        [BindProperty(SupportsGet = true)]
        public int? ParentId { get; set; }

        [BindProperty(SupportsGet = true)]
        public ProjectParentType? ParentType { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? AppId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? TeamId { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? FilterIsActive { get; set; } = true;

        [BindProperty(SupportsGet = true)]
        public HungReason? FilterHungReason { get; set; }

        public IEnumerable<Team> Teams { get; set; }
        public ILookup<int, AppInfoResult> AppInfo { get; set; }
        public ILookup<int, ProjectInfoResult> ProjectInfo { get; set; } // keyed to teamId
        public ILookup<int, ProjectInfoResult> ProjectsWithoutApps { get; set; } // keyed to teamId
        public IEnumerable<HungItemInfo> HungItems { get; set; }

        public Application Application { get; set; }
        public IEnumerable<ProjectInfoResult> AppProjects { get; set; }        

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            if (!TeamId.HasValue) TeamId = CurrentOrgUser.CurrentTeamId;

            if (ParentId.HasValue && ParentType.HasValue)
            {
                switch (ParentType)
                {
                    case ProjectParentType.Application:
                        AppId = ParentId;
                        break;

                    case ProjectParentType.Team:
                        TeamId = ParentId;
                        break;
                }
            }

            if (AppId.HasValue)
            {
                Application = await connection.FindAsync<Application>(AppId.Value);
                AppProjects = await new ProjectInfo() { OrgId = OrgId, AppId = AppId, IsActive = FilterIsActive }.ExecuteAsync(connection);
                HungItems = WorkItems
                    .Where(wi => wi.IsHung)
                    .GroupBy(row => row.HungReason)
                    .Select(grp =>
                    {
                        var result = HungItemInfo.Dictionary[grp.Key];
                        result.Count = grp.Count();
                        return result;
                    });

                if (FilterHungReason.HasValue)
                {
                    WorkItems = WorkItems.Where(wi => wi.HungReason == FilterHungReason);
                }
            }
            else
            {
                Teams = await new Teams() { OrgId = OrgId, IsActive = FilterIsActive, Id = TeamId }.ExecuteAsync(connection);

                var apps = await new AppInfo() { OrgId = OrgId, TeamId = TeamId, IsActive = FilterIsActive }.ExecuteAsync(connection);
                AppInfo = apps.ToLookup(row => row.TeamId ?? 0);

                var projects = await new ProjectInfo() { OrgId = OrgId, TeamUsesApplications = false, IsActive = FilterIsActive }.ExecuteAsync(connection);
                ProjectInfo = projects.ToLookup(row => row.TeamId);

                var projectsWithoutApps = await new ProjectInfo() { OrgId = OrgId, IsActive = FilterIsActive, HasApplicationId = false }.ExecuteAsync(connection);
                ProjectsWithoutApps = projectsWithoutApps.ToLookup(row => row.TeamId);
            }
        }

        protected override OpenWorkItems GetQuery()
        {
            if (CurrentOrgUser.CurrentAppId.HasValue) AppId = CurrentOrgUser.CurrentAppId.Value;

            if (AppId.HasValue)
            {
                return new OpenWorkItems(QueryTraces)
                {
                    OrgId = OrgId,
                    LabelId = LabelId,
                    AppId = AppId,
                    HasProject = false
                };
            }

            return null;
        }

        public async Task<IActionResult> OnPostCreateProjectAsync(int teamId, int? applicationId, string name)
        {
            var project = new Project() { Name = name, ApplicationId = applicationId, TeamId = teamId };
            await Data.TrySaveAsync(project);
            return Redirect($"/Dashboard/Projects/{project.Id}");
        }
    }
}