using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Work
{
	[Authorize]
	public class AllItemsModel : AppPageModel, IWorkItemDashboard
	{
		public AllItemsModel(IConfiguration config) : base(config)
		{
		}		

		public IEnumerable<AllWorkItemsResult> WorkItems { get; set; }
		public IEnumerable<Activity> Activities { get; set; }
		public IEnumerable<WorkItemSize> Sizes { get; set; }
		public IEnumerable<CloseReason> CloseReasons { get; set; }
		public IEnumerable<Milestone> Milestones { get; set; }

		public async Task OnGetAsync()
		{
			using (var cn = Data.GetConnection())
			{
				WorkItems = await new AllWorkItems() { OrgId = Data.CurrentOrg.Id }.ExecuteAsync(cn);
				Activities = await new Activities() { OrgId = Data.CurrentOrg.Id, IsActive = true }.ExecuteAsync(cn);
				Sizes = await new WorkItemSizes() { OrgId = Data.CurrentOrg.Id }.ExecuteAsync(cn);
				CloseReasons = await new CloseReasons().ExecuteAsync(cn);
				Milestones = await new Milestones() { OrgId = Data.CurrentOrg.Id }.ExecuteAsync(cn);
			}
		}
	}
}