using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Dashboard
{
	[Authorize]
	public class MyItemsModel : DashboardPageModel
	{
		public MyItemsModel(IConfiguration config) : base(config)
		{
		}

		protected override AllWorkItems GetQuery()
		{
			return new AllWorkItems() { OrgId = OrgId, AssignedUserId = UserId };
		}
	}
}