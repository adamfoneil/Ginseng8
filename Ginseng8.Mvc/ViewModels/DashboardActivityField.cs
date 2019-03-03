using Ginseng.Models;
using Ginseng.Mvc.Queries;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class DashboardActivityField
	{
		public AllWorkItemsResult Item { get; set; }
		public IEnumerable<Activity> Activities { get; set; }
	}
}