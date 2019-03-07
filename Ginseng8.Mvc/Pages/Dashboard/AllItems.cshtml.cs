using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Work
{
	[Authorize]
	public class AllItemsModel : AppPageModel
	{
		public AllItemsModel(IConfiguration config) : base(config)
		{
		}

		public IEnumerable<AllWorkItemsResult> WorkItems { get; set; }
		public CommonDropdowns Dropdowns { get; set; }

		public async Task OnGetAsync()
		{
			using (var cn = Data.GetConnection())
			{
				WorkItems = await new AllWorkItems() { OrgId = OrgId }.ExecuteAsync(cn);
				Dropdowns = await CommonDropdowns.FillAsync(cn, OrgId, CurrentOrgUser.Responsibilities);
			}
		}
	}
}