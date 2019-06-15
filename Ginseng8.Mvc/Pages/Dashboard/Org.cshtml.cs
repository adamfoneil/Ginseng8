using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Dashboard
{
    public class OrgModel : DashboardPageModel
    {
        public OrgModel(IConfiguration config) : base(config)
        {
            ShowExcelDownload = false;
            ShowLabelFilter = false;
        }

        [BindProperty(SupportsGet = true)]
        public int? AppId { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? FilterIsActive { get; set; } = true;

        public IEnumerable<Team> Teams { get; set; }
        public ILookup<int, AppInfoResult> AppInfo { get; set; }
        public ILookup<int, ProjectInfoResult> ProjectInfo { get; set; }
        
        public Application Application { get; set; }
        public IEnumerable<ProjectInfoResult> AppProjects { get; set; }

        protected override async Task OnGetInternalAsync(SqlConnection connection)
        {
            if (AppId.HasValue)
            {
                Application = await connection.FindAsync<Application>(AppId.Value);                
                AppProjects = await new ProjectInfo() { OrgId = OrgId, AppId = AppId }.ExecuteAsync(connection);
            }
            else
            {
                Teams = await new Teams() { OrgId = OrgId, IsActive = FilterIsActive, Id = CurrentOrgUser.CurrentTeamId }.ExecuteAsync(connection);

                var apps = await new AppInfo() { OrgId = OrgId, TeamId = CurrentOrgUser.CurrentTeamId, IsActive = FilterIsActive }.ExecuteAsync(connection);
                AppInfo = apps.ToLookup(row => row.TeamId ?? 0);

                var projects = await new ProjectInfo() { OrgId = OrgId, TeamUsesApplications = false }.ExecuteAsync(connection);
                ProjectInfo = projects.ToLookup(row => row.TeamId ?? 0);
            }
        }

        protected override OpenWorkItems GetQuery()
        {
            if (AppId.HasValue)
            {
                return new OpenWorkItems(QueryTraces)
                {
                    OrgId = OrgId,
                    LabelId = LabelId,
                    AppId = AppId
                };
            }

            return null;
        }

        public async Task<IActionResult> OnPostCreateProjectAsync(int teamId, int? applicationId, string name)
        {
            var project = new Project() { Name = name, ApplicationId = applicationId, TeamId = teamId };
            await Data.TrySaveAsync(project);            
            return (applicationId.HasValue) ?
                Redirect($"Org?appId={applicationId}") :
                Redirect($"Org?teamId={teamId}");
        }

    }
}