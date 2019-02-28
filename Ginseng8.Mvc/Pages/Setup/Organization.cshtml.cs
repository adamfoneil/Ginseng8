using Ginseng.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	[Authorize]
	public class OrganizationModel : AppPageModel
	{				
		public OrganizationModel(IConfiguration config) : base(config)
		{			
		}

		[BindProperty]
		public Organization Organization { get; set; }

		public void OnGet()
		{
			Organization = CurrentOrg ?? new Organization();
		}

		public async Task<ActionResult> OnPostAsync()
		{
			await TrySaveAsync(Organization);			
			return RedirectToPage("/Setup/Organization");
		}

	}
}