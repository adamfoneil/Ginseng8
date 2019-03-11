using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	public class MilestonesModel : AppPageModel
	{
		public MilestonesModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<Milestone> Milestones { get; set; }

		public void OnGet()
		{
			using (var cn = Data.GetConnection())
			{
				Milestones = new Milestones() { OrgId = CurrentOrg.Id }.Execute(cn);
			}
		}

		public async Task<ActionResult> OnPostSave(Milestone record)
		{
			record.OrganizationId = CurrentOrg.Id;
			await Data.TrySaveAsync(record);
			return RedirectToPage("/Setup/Milestones");
		}

		public async Task<ActionResult> OnPostDelete(int id)
		{
			await Data.TryDelete<Milestone>(id);
			return RedirectToPage("/Setup/Milestones");
		}
	}
}