using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	public class SizesModel : AppPageModel
	{
		public SizesModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<WorkItemSize> WorkItemSizes { get; set; }

		public void OnGet()
		{
			using (var cn = Data.GetConnection())
			{
				WorkItemSizes = new WorkItemSizes() { OrgId = Data.CurrentOrg.Id }.Execute(cn);
			}
		}

		public async Task<ActionResult> OnPostSave(WorkItemSize record)
		{
			record.OrganizationId = Data.CurrentOrg.Id;
			await Data.TrySaveAsync(record);
			return RedirectToPage("/Setup/Sizes");
		}

		public async Task<ActionResult> OnPostDelete(int id)
		{
			await Data.TryDelete<WorkItemSize>(id);
			return RedirectToPage("/Setup/Sizes");
		}
	}
}