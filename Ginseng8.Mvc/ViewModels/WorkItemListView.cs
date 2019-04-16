using Ginseng.Models;
using Ginseng.Mvc.Queries;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.ViewModels
{
	/// <summary>
	/// should be obsolete, see <see cref="Pages.WorkItem.ListModel"/>
	/// </summary>
	public class WorkItemListView
	{
		public IEnumerable<OpenWorkItemsResult> WorkItems { get; set; }
		public ILookup<int, Label> SelectedLabels { get; set; }
	}
}