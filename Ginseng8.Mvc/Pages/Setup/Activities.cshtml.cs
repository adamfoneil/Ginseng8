using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	public class ActivitiesModel : AppPageModel
	{
		public ActivitiesModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<Activity> Activities { get; set; }

		public void OnGet(bool isActive = true)
		{
			using (var cn = Data.GetConnection())
			{
				Activities = new Activities() { OrgId = CurrentOrg.Id, IsActive = isActive }.Execute(cn);
			}
		}

		public async Task<ActionResult> OnPostSaveAsync(Activity record)
		{
			record.OrganizationId = CurrentOrg.Id;
			await Data.TrySaveAsync(record);
			return RedirectToPage("/Setup/Activities");
		}

		public async Task<ActionResult> OnPostDelete(int id)
		{
			await Data.TryDelete<Activity>(id);
			return RedirectToPage("/Setup/Activities");
		}
	}
}