using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItem
{
	public class CreateModel : AppPageModel
	{
		public CreateModel(IConfiguration config) : base(config)
		{
		}

		public Models.WorkItem WorkItem { get; private set; }

		public async Task OnPostAsync(Models.WorkItem workItem)
		{
			// make sure incoming number has not been set so that SetNumber method works
			workItem.Number = 0;

			// make sure item is part of this org
			workItem.OrganizationId = Data.CurrentOrg.Id;

			await Data.TrySaveAsync(workItem, async (cn, r) =>
			{
				await r.SetNumber(cn);
			});

			WorkItem = workItem;
		}
	}
}