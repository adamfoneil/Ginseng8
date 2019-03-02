using Ginseng.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItems
{
	public class CreateModel : AppPageModel
	{
		public CreateModel(IConfiguration config) : base(config)
		{
		}

		public WorkItem WorkItem { get; private set; }

		public async Task OnPostAsync(WorkItem workItem)
		{
			workItem.Number = 0; // make sure incoming number has not been set
			workItem.OrganizationId = Data.CurrentOrg.Id;
			await Data.TrySaveAsync(workItem, async (cn, r) =>
			{
				await r.SetNumber(cn);
			});
			WorkItem = workItem;
		}
	}
}