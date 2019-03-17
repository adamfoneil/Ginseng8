using Ginseng.Mvc.Queries;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class FilteredItemView
	{
		public DashboardPageModel Page { get; set; }
		public IEnumerable<OpenWorkItemsResult> WorkItems { get; set; }
	}
}