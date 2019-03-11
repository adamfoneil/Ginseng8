using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	public class LabelsModel : AppPageModel
	{
		public LabelsModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<Label> Labels { get; set; }

		public void OnGet(bool isActive = true)
		{
			using (var cn = Data.GetConnection())
			{
				Labels = new Labels() { OrgId = OrgId, IsActive = isActive }.Execute(cn);
			}
		}

		public async Task<ActionResult> OnPostSave(Label label)
		{
			label.OrganizationId = OrgId;
			await Data.TrySaveAsync(label);
			return RedirectToPage("/Setup/Labels");
		}

		public async Task<ActionResult> OnPostDelete(int id)
		{
			await Data.TryDelete<Label>(id);
			return RedirectToPage("/Setup/Labels");
		}
	}
}