using Dapper;
using Ginseng.Models;
using Ginseng.Mvc.Attributes;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	[Authorize]
	[OrgNotRequired]
	public class OrganizationModel : AppPageModel
	{
		public OrganizationModel(IConfiguration config) : base(config)
		{
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
			using (var cn = Data.GetConnection())
			{
				await cn.ExecuteAsync(
					@"INSERT INTO [dbo].[OrganizationUser] (
						[OrganizationId], [UserId], [IsEnabled], [IsRequest], [Responsibilities], [WorkDays], [DailyWorkHours], [CreatedBy], [DateCreated]
					) SELECT
						[org].[Id], @userId, 0, 1, 0, 0, 0, [u].[UserName], GETUTCDATE()
					FROM
						[dbo].[Organization] [org],
						[dbo].[AspNetUsers] [u]
					WHERE
						NOT EXISTS(SELECT 1 FROM [dbo].[OrganizationUser] WHERE [OrganizationId]=[org].[Id] AND [UserId]=@userId) AND
						[org].[Name]=@orgName AND
						[u].[UserId]=@userId", new { orgName = name, userId = UserId });
			}

			return RedirectToPage("/Setup/Organization");
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