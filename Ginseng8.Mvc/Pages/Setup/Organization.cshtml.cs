using Ginseng.Models;
using Ginseng.Mvc.Attributes;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	[Authorize]
	[OrgNotRequired]
	public class OrganizationModel : AppPageModel
	{
		private readonly Email _email;
		private readonly IConfiguration _config;
		private readonly IViewRenderService _viewRender;

		public OrganizationModel(
			IConfiguration config,
			IViewRenderService viewRender)
			: base(config)
		{
			_config = config;
			_email = new Email(config);
			_viewRender = viewRender;
		}

		[BindProperty]
		public Organization Organization { get; set; }

		public IEnumerable<OrganizationUser> JoinRequests { get; set; }

		/// <summary>
		/// Indicates user must create or join an organization
		/// </summary>
		public bool MustCreateOrg { get; set; }

		public IEnumerable<Organization> MyOrgs { get; set; }

		public SelectList DayOfWeekSelect { get; set; }

		public void OnGet(bool mustCreate = false)
		{
			if (mustCreate) MustCreateOrg = true;

			Organization = CurrentOrg ?? new Organization();
			using (var cn = Data.GetConnection())
			{
				MyOrgs = new MyOrgs() { UserId = UserId }.Execute(cn);
				JoinRequests = new MyOrgUsers() { UserId = UserId, IsRequest = true, IsEnabled = false }.Execute(cn);
				DayOfWeekSelect = new WorkDaySelect().ExecuteSelectList(cn);
			}
		}

		public async Task<IActionResult> OnPostJoinAsync(string name)
		{
			try
			{
				using (var cn = Data.GetConnection())
				{
					int orgUserId = await new CreateOrgUserJoinRequest() { OrgName = name, UserId = UserId }.ExecuteSingleAsync(cn);
					var orgUser = await cn.FindAsync<OrganizationUser>(orgUserId);

					var page = new EmailContent.JoinRequestModel(_config);
					await page.OnGetAsync(orgUserId);
					var content = await _viewRender.RenderAsync("Pages.EmailContent.JoinRequest", page);
					await _email.SendAsync(orgUser.Organization.OwnerUser.Email, "Ginseng: Org Join Request", content);
				}
				Data.ClearErrorMessage();
			}
			catch (Exception)
			{
				Data.SetErrorMessage("Error requesting org. You might have misspelled it.");
			}

			return RedirectToPage("/Setup/Organization");
		}

		public async Task<IActionResult> OnGetDeleteJoinRequestAsync(int id)
		{
			using (var cn = Data.GetConnection())
			{
				var orgUser = await cn.FindAsync<OrganizationUser>(id);
				if (orgUser.UserId != UserId) return BadRequest();
				if (!orgUser.IsRequest) return BadRequest();
				await cn.DeleteAsync<OrganizationUser>(orgUser.Id);
				return RedirectToPage("/Setup/Organization");
			}
		}

		public async Task<ActionResult> OnPostAsync()
		{
			await Data.TrySaveAsync(Organization, successMessage: "Organization was updated succesfully.");
			return RedirectToPage("/Setup/Organization");
		}

		public async Task<ActionResult> OnPostSave(Organization org)
		{
			if (org.Id == 0) org.OwnerUserId = CurrentUser.UserId;
			await Data.TrySaveAsync(org);
			return RedirectToPage("/Setup/Organization");
		}

		public async Task<ActionResult> OnPostDelete(int id)
		{
			await Data.TryDelete<Organization>(id);
			return RedirectToPage("/Setup/Organization");
		}
	}
}