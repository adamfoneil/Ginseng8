using System;
using Ginseng.Mvc.Queries;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Work
{
	public class TeamModel : DashboardPageModel
	{
		public TeamModel(IConfiguration config) : base(config)
		{
		}

		protected override Func<ClosedWorkItemsResult, int> ClosedItemGrouping => (item) => item.DeveloperUserId ?? 0;

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems()
			{
				OrgId = OrgId,
				AppId = CurrentOrgUser.CurrentAppId,
				LabelId = LabelId
			};
		}
	}
}