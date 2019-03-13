using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.WorkItem
{
	public class EditModel : AppPageModel
	{
		public EditModel(IConfiguration config) : base(config)
		{
		}

		public OpenWorkItemsResult Item { get; set; }
		public CommonDropdowns Dropdowns { get; set; }

		public async Task OnGetAsync(int id)
		{
			using (var cn = Data.GetConnection())
			{
				Item = await new OpenWorkItems() { OrgId = OrgId, Number = id }.ExecuteSingleAsync(cn);
				Dropdowns = await CommonDropdowns.FillAsync(cn, OrgId, CurrentOrgUser.Responsibilities);
			}
		}
	}
}