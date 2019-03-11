using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItem
{
	public class SetEstimateModel : AppPageModel
	{
		public SetEstimateModel(IConfiguration config) : base(config)
		{
		}

		public DashboardEstimateField Field { get; set; }

		public async Task OnPostAsync(int number, int sizeId)
		{
			using (var cn = Data.GetConnection())
			{
				var item = await Data.FindWhereAsync<Models.WorkItem>(cn, new { OrganizationId = OrgId, Number = number });

				WorkItemSize size = null;
				if (sizeId != 0)
				{
					size = await Data.FindWhereAsync<WorkItemSize>(cn, new { OrganizationId = OrgId, Id = sizeId });

					if (item != null)
					{
						if (size != null)
						{
							item.SizeId = size.Id;
						}
						else
						{
							item.SizeId = null;
						}
					}

					await Data.TrySaveAsync(cn, item);
				}

				Field = new DashboardEstimateField()
				{
					Item = await new AllWorkItems() { OrgId = OrgId, Number = number }.ExecuteSingleAsync(cn),
					Sizes = await new WorkItemSizes() { OrgId = OrgId }.ExecuteAsync(cn)
				};
			}
		}
	}
}