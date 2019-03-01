using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	public class ApplicationsModel : AppPageModel
	{
		public ApplicationsModel(IConfiguration config) : base(config)
		{
		}

		[BindProperty]
		public Application NewApplication { get; set; }

		public bool IsActive { get; set; }

		public IEnumerable<Application> Applications { get; set; }

		public async Task OnGetAsync(bool isActive = true)
		{
			IsActive = true;

			using (var cn = GetConnection())
			{
				Applications = await new Applications() { OrgId = CurrentOrg.Id, IsActive = isActive }.ExecuteAsync(cn);
			}
		}

		public async Task<ActionResult> OnPostCreateApp()
		{
			NewApplication.OrganizationId = CurrentOrg.Id;
			await TrySaveAsync(NewApplication);
			return RedirectToPage("/Setup/Applications");
		}

		public async Task<ActionResult> OnGetDelete(int id)
		{
			await TryDelete<Application>(id);
			return RedirectToPage("/Setup/Applications");
		}
		
		public async Task<ActionResult> Save(Application app)
		{
			app.OrganizationId = CurrentOrg.Id;
			await TrySaveAsync(app);
			return RedirectToPage("/Setup/Applications");
		}
	}
}