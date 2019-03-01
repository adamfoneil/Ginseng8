using Ginseng.Models;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	[Authorize]
	public class OrgUserModel : AppPageModel
	{
		public OrgUserModel(IConfiguration config) : base(config)
		{
		}

		public SelectList MyOrgSelect { get; set; }

		[BindProperty]
		public OrganizationUser OrgUser { get; set; }

		[BindProperty]
		public int[] SelectedWorkDays { get; set; }

		public WorkDay[] WorkDays { get; set; }

		public void OnGet()
		{
			WorkDays = WorkDay.WorkDays.ToArray();

			using (var cn = GetConnection())
			{
				MyOrgSelect = new MyOrgSelect() { UserId = CurrentUser.UserId }.ExecuteSelectList(cn, CurrentUser.OrganizationId);
			}

			OrgUser = CurrentOrgUser ?? new OrganizationUser()
			{
				UserId = CurrentUser.UserId,
				OrganizationId = CurrentOrg.Id,
				DisplayName = User.Identity.Name,
				WorkDays = 62 // mon -> fri flag values
			};
		}

		public async Task<ActionResult> OnPostAsync()
		{
			OrgUser.OrganizationId = CurrentOrg.Id;
			OrgUser.UserId = CurrentUser.UserId;

			await TrySaveAsync(OrgUser, new string[]
			{
				nameof(OrganizationUser.OrganizationId),
				nameof(OrganizationUser.UserId),
				nameof(OrganizationUser.DisplayName),
				nameof(OrganizationUser.MaxWorkInProgress),
				nameof(OrganizationUser.DailyWorkHours),
				nameof(OrganizationUser.WorkDays)
			}, "Record updated successfully.");
			
			return RedirectToPage("/Setup/OrgUser");
		}

		public async Task<ActionResult> OnSelectOrg(int orgId)
		{
			CurrentUser.OrganizationId = orgId;
			await TryUpdateAsync(CurrentUser, r => r.OrganizationId);
			return RedirectToPage("/Setup/OrgUser");
		}
	}
}