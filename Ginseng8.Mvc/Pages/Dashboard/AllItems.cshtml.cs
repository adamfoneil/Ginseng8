using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
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
		public IEnumerable<Activity> AllActivities { get; set; }
		public IEnumerable<WorkItemSize> Sizes { get; set; }
		public IEnumerable<CloseReason> CloseReasons { get; set; }
		public IEnumerable<Milestone> Milestones { get; set; }
		public IEnumerable<Activity> MyActivities { get; set; }
		
		public async Task OnGetAsync()
		{
			using (var cn = Data.GetConnection())
			{
				WorkItems = await new AllWorkItems() { OrgId = OrgId }.ExecuteAsync(cn);
				AllActivities = await new Activities() { OrgId = OrgId, IsActive = true }.ExecuteAsync(cn);
				MyActivities = await new Activities() { OrgId = OrgId, IsActive = true, Responsibilities = CurrentOrgUser.Responsibilities }.ExecuteAsync(cn);
				Sizes = await new WorkItemSizes() { OrgId = OrgId }.ExecuteAsync(cn);
				CloseReasons = await new CloseReasons().ExecuteAsync(cn);
				Milestones = await new Milestones() { OrgId = OrgId }.ExecuteAsync(cn);
			}
		}
	}
}