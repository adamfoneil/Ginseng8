using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.Pages.Work
{
	[Authorize]
	public class IndexModel : DashboardPageModel
	{
		public IndexModel(IConfiguration config) : base(config)
		{
			ShowLabelFilter = false;
		}

		public IEnumerable<OpenWorkItemsResult> ImpededItems
		{			
			get { return WorkItems.Where(wi => wi.HasImpediment); }
		}

		public IEnumerable<OpenWorkItemsResult> PausedItems
		{
			get { return WorkItems.Where(wi => wi.IsPaused()); }			
		}

		public IEnumerable<OpenWorkItemsResult> StoppedItems
		{
			get { return WorkItems.Where(wi => wi.IsStopped()); }			
		}

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems() { OrgId = OrgId, AppId = CurrentOrgUser.CurrentAppId };
		}
	}
}