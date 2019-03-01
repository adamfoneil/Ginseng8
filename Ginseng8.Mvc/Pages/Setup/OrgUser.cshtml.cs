using Ginseng.Models;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Ginseng.Mvc.Pages.Setup
{
	public class OrgUserModel : AppPageModel
	{
		public OrgUserModel(IConfiguration config) : base(config)
		{
		}

		public SelectList MyOrgSelect { get; set; }

		[BindProperty]
		public OrganizationUser OrgUser { get; set; }

		public void OnGet()
		{
			using (var cn = GetConnection())
			{
				MyOrgSelect = new MyOrgSelect() { UserId = CurrentUser.UserId }.ExecuteSelectList(cn, CurrentUser.OrganizationId);
			}
				
			OrgUser = CurrentOrgUser ?? new OrganizationUser() { UserId = CurrentUser.UserId, OrganizationId = CurrentOrg.Id };
		}
	}
}