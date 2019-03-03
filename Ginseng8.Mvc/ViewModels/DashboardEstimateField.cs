using Ginseng.Models;
using Ginseng.Mvc.Queries;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class DashboardEstimateField
	{
		public AllWorkItemsResult Item { get; set; }
		public IEnumerable<WorkItemSize> Sizes { get; set; }
	}
}