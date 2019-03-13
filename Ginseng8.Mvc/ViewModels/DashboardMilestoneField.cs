using Ginseng.Models;
using Ginseng.Mvc.Queries;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class DashboardMilestoneField
	{
		public OpenWorkItemsResult Item { get; set; }
		public IEnumerable<Milestone> Milestones { get; set; }
	}
}