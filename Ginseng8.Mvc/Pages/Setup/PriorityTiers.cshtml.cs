using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	public class PriorityTiersModel : AppPageModel
	{
		public PriorityTiersModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<PriorityTier> PriorityTiers { get; set; }

		public async Task OnGetAsync()
		{
			using (var cn = Data.GetConnection())
			{
				PriorityTiers = await new PriorityTiers() { OrgId = OrgId }.ExecuteAsync(cn);
			}
		}

		public async Task<ActionResult> OnPostSave(PriorityTier record)
		{
			record.OrganizationId = OrgId;
			await Data.TrySaveAsync(record);
			return RedirectToPage("/Setup/PriorityTiers");
		}

		public async Task<ActionResult> OnPostDelete(int id)
		{
			await Data.TryDelete<PriorityTier>(id);
			return RedirectToPage("/Setup/PriorityTiers");
		}

	}
}