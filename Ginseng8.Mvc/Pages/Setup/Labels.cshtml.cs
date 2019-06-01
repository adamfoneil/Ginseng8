using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
	public class LabelsModel : AppPageModel
	{
		public LabelsModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<Label> Labels { get; set; }
        public Dictionary<int, LabelSubscription> Subscriptions { get; set; }

        public async Task OnGetAsync(bool isActive = true)
		{
			using (var cn = Data.GetConnection())
			{
				Labels = await new Labels() { OrgId = OrgId, IsActive = isActive }.ExecuteAsync(cn);

                var subscriptions = await new MyLabelSubscriptions() { OrgId = OrgId, UserId = UserId }.ExecuteAsync(cn);
                Subscriptions = subscriptions.ToDictionary(row => row.LabelId);
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
			await Data.TryDeleteAsync<Label>(id);
			return RedirectToPage("/Setup/Labels");
		}
	}
}