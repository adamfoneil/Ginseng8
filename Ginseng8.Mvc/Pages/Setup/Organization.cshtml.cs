using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
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

		public IEnumerable<Organization> MyOrgs { get; set; }

		public void OnGet()
		{
			Organization = CurrentOrg ?? new Organization();
			using (var cn = GetConnection())
			{
				MyOrgs = new MyOrgs() { UserId = CurrentUser.UserId }.Execute(cn);
			}				
		}

		public async Task<ActionResult> OnPostAsync()
		{
			await TrySaveAsync(Organization, "Organization was updated succesfully.");			
			return RedirectToPage("/Setup/Organization");
		}

		public async Task<ActionResult> OnPostSave(Organization org)
		{
			org.OwnerUserId = CurrentUser.UserId;
			await TrySaveAsync(org);
			return RedirectToPage("/Setup/Organization");
		}

		public async Task<ActionResult> OnPostDelete(int id)
		{
			await TryDelete<Organization>(id);
			return RedirectToPage("/Setup/Organization");
		}
	}
}