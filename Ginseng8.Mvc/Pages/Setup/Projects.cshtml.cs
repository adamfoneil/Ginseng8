using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	public class ProjectsModel : AppPageModel
	{
		public ProjectsModel(IConfiguration config) : base(config)
		{
		}

        [BindProperty(SupportsGet = true)]
        public bool IsActive { get; set; } = true;

        [BindProperty(SupportsGet = true)]
        public int TeamId { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? AppId { get; set; }

        public bool UseApplications { get; set; }

        public SelectList TeamSelect { get; set; }
		public SelectList AppSelect { get; set; }

		public IEnumerable<Project> Projects { get; set; }

		public async Task OnGetAsync()
		{
			using (var cn = Data.GetConnection())
			{
                if (TeamId == 0) AppId = null;

                if (TeamId != 0)
                {
                    var team = await cn.FindAsync<Team>(TeamId);
                    UseApplications = team.UseApplications;
                    if (!UseApplications && AppId.HasValue) AppId = null;
                }                

                TeamSelect = await new TeamSelect() { OrgId = OrgId }.ExecuteSelectListAsync(cn, TeamId);
				AppSelect = await new AppSelect() { OrgId = OrgId, TeamId = TeamId }.ExecuteSelectListAsync(cn, AppId);
				Projects = await new Projects() { IsActive = IsActive, AppId = AppId, TeamId = TeamId }.ExecuteAsync(cn);
			}
		}

		public async Task<ActionResult> OnPostSave(Project record)
		{
            if (record.ApplicationId == 0) record.ApplicationId = null;
			await Data.TrySaveAsync(record, new string[] { "TeamId", "ApplicationId", "Name", "IsActive", "Nickname", "Priority" });
			return Redirect($"/Setup/Projects?TeamId={record.TeamId}&AppId={record.ApplicationId}");
		}

		public async Task<ActionResult> OnPostDelete(int id)
		{
			var prj = await Data.FindAsync<Project>(id);
			await Data.TryDeleteAsync<Project>(id);
			return Redirect($"/Setup/Projects?TeamId={prj.TeamId}&AppId={prj.ApplicationId}");
		}
	}
}