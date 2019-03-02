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

		public WorkDay[] WorkDays { get; set; }

		public void OnGet()
		{
			WorkDays = WorkDay.WorkDays.ToArray();

			using (var cn = Data.Open())
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

		public async Task<ActionResult> OnPostAsync(int[] selectedWorkDays)
		{
			OrgUser.Id = CurrentOrgUser?.Id ?? 0;
			OrgUser.OrganizationId = CurrentOrg.Id;
			OrgUser.UserId = CurrentUser.UserId;
			OrgUser.WorkDays = selectedWorkDays?.Sum() ?? 0;			

			var fields = new string[]
			{
				nameof(OrganizationUser.OrganizationId),
				nameof(OrganizationUser.UserId),
				nameof(OrganizationUser.DisplayName),
				nameof(OrganizationUser.MaxWorkInProgress),
				nameof(OrganizationUser.DailyWorkHours),
				nameof(OrganizationUser.WorkDays)				
			}.ToList();			

			if (OrgUser.Id == 0)
			{
				fields.Add(nameof(OrganizationUser.IsEnabled));
				fields.Add(nameof(OrganizationUser.IsRequest));
			}

			await Data.TrySaveAsync(OrgUser, fields.ToArray(), "Record updated successfully.");
			
			return RedirectToPage("/Setup/OrgUser");
		}

		public async Task<ActionResult> OnPostSelectOrg(int orgId)
		{
			CurrentUser.OrganizationId = orgId;
			await Data.TryUpdateAsync(CurrentUser, r => r.OrganizationId);
			return RedirectToPage("/Setup/OrgUser");
		}
	}
}