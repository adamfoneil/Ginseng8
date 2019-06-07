using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
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
		public int AppId { get; set; }

		public SelectList AppSelect { get; set; }

		public IEnumerable<Project> Projects { get; set; }

		public void OnGet(bool isActive = true)
		{
			using (var cn = Data.GetConnection())
			{
				AppSelect = new AppSelect() { OrgId = OrgId }.ExecuteSelectList(cn, AppId);
				Projects = new Projects() { IsActive = isActive, AppId = AppId }.Execute(cn);
			}
		}

		public async Task<ActionResult> OnPostSave(Project record)
		{			
			await Data.TrySaveAsync(record, new string[] { "ApplicationId", "Name", "IsActive", "Nickname", "Priority" });
			return Redirect($"/Setup/Projects?AppId={record.ApplicationId}");
		}

		public async Task<ActionResult> OnPostDelete(int id)
		{
			var prj = await Data.FindAsync<Project>(id);
			await Data.TryDeleteAsync<Project>(id);
			return Redirect($"/Setup/Projects?AppId={prj.ApplicationId}");
		}
	}
}