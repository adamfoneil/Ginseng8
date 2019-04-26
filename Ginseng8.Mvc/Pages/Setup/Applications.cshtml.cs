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

		public bool IsActive { get; set; }

		public IEnumerable<Application> Applications { get; set; }

		public async Task OnGetAsync(bool isActive = true)
		{
			IsActive = true;

			using (var cn = Data.GetConnection())
			{
				Applications = await new Applications() { OrgId = CurrentOrg.Id, IsActive = isActive }.ExecuteAsync(cn);
			}
		}

		public async Task<ActionResult> OnPostSave(Application app)
		{
			app.OrganizationId = CurrentOrg.Id;
			await Data.TrySaveAsync(app);
			return RedirectToPage("/Setup/Applications");
		}

		public async Task<ActionResult> OnPostDelete(int id)
		{
			await Data.TryDeleteAsync<Application>(id);
			return RedirectToPage("/Setup/Applications");
		}
	}
}