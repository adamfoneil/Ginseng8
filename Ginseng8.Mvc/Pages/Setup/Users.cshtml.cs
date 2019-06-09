using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	public class UsersModel : AppPageModel
	{
		public UsersModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<OrganizationUser> Users { get; set; }

        public ILookup<int, string> WorkDays { get; set; }

		public async Task OnGetAsync()
		{
			using (var cn = Data.GetConnection())
			{
				Users = await new MyOrgUsers(QueryTraces) { OrgId = OrgId, ExcludeUserId = UserId, ExcludeOwner = true, HoursOrgId = OrgId }.ExecuteAsync(cn);

                var workDays = await new UserWorkDays() { OrgId = OrgId }.ExecuteAsync(cn);
                WorkDays = workDays.ToLookup(row => row.UserId, row => row.Abbreviation);
            }
		}

		public async Task<IActionResult> OnPostSave(OrganizationUser record)
		{
			var orgUser = await Data.FindAsync<OrganizationUser>(record.Id);

			if (CurrentOrg.OwnerUserId == orgUser.UserId && CurrentOrg.Id == orgUser.OrganizationId)
			{
				throw new Exception("Can't modify the organization owner account.");
			}

			orgUser.IsEnabled = record.IsEnabled;

			if (record.IsRequest) orgUser.IsRequest = false;			

			if (await Data.TryUpdateAsync(orgUser, r => r.IsEnabled, r => r.IsRequest))
			{
				if (record.IsEnabled && !orgUser.UserProfile.OrganizationId.HasValue)
				{
					orgUser.UserProfile.OrganizationId = OrgId;
					await Data.TryUpdateAsync(orgUser.UserProfile, r => r.OrganizationId);
				}
			}

			return RedirectToPage("/Setup/Users");
		}

		public async Task<IActionResult> OnPostDelete(int id)
		{
			var orgUser = await Data.FindAsync<OrganizationUser>(id);

			if (CurrentOrg.OwnerUserId == orgUser.UserId && CurrentOrg.Id == orgUser.OrganizationId)
			{
				throw new Exception("Can't modify the organization owner account.");
			}

			await Data.TryDeleteAsync<OrganizationUser>(id);
			return RedirectToPage("/Setup/Users");
		}
	}
}