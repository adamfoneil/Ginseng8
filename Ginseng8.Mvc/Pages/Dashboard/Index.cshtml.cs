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

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems()
			{
				OrgId = OrgId,
				AssignedUserId = UserId,
				AppId = CurrentOrgUser.CurrentAppId,
				LabelId = LabelId
			};
		}
	}
}