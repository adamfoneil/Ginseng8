using Ginseng.Models;
using Ginseng.Mvc.Queries;
using System.Collections.Generic;

namespace Ginseng.Mvc.Interfaces
{
	public interface IWorkItemDashboard
	{
		IEnumerable<AllWorkItemsResult> WorkItems { get; }
		IEnumerable<Activity> AllActivities { get; }
		IEnumerable<WorkItemSize> Sizes { get; }
		IEnumerable<CloseReason> CloseReasons { get; }
		IEnumerable<Milestone> Milestones { get; }
	}
}