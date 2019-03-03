using Ginseng.Models;
using Ginseng.Mvc.Queries;
using System.Collections.Generic;

namespace Ginseng.Mvc.Interfaces
{
	public interface IWorkItemDashboard
	{
		IEnumerable<AllWorkItemsResult> WorkItems { get; }
		IEnumerable<Activity> Activities { get; }
		IEnumerable<WorkItemSize> Sizes { get; }
	}
}