using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
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

		public IEnumerable<Project> Projects { get; set; }

		public void OnGet(bool isActive = true)
		{
			using (var cn = Data.Open())
			{
				Projects = new Projects() { OrgId = CurrentOrg.Id, IsActive = isActive }.Execute(cn);
			}
		}

		public async Task<ActionResult> OnPostSave(Project record)
		{
			record.OrganizationId = CurrentOrg.Id;
			await Data.TrySaveAsync(record);
			return RedirectToPage("/Setup/Projects");
		}

		public async Task<ActionResult> OnPostDelete(int id)
		{
			await Data.TryDelete<Project>(id);
			return RedirectToPage("/Setup/Projects");
		}
	}
}