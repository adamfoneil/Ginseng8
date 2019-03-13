using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
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

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId };
		}

		protected override async Task OnGetInternalAsync(SqlConnection connection)
		{
			int[] projectIds = WorkItems
				.GroupBy(row => row.ProjectId)
				.Select(grp => grp.Key).ToArray();

			var projects = await new Projects() { OrgId = OrgId, IsActive = true, IncludeIds = projectIds }.ExecuteAsync(connection);
			Projects = projects.ToDictionary(row => row.Id);
		}
	}
}