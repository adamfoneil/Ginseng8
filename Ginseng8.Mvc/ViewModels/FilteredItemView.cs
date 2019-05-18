using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.ViewModels
{
	public class FilteredItemView
	{
		public DashboardPageModel Page { get; set; }
		public IEnumerable<OpenWorkItemsResult> WorkItems { get; set; }
		public ILookup<int, Comment> Comments { get; set; }
        public IEnumerable<SelectListItem> AssignToUsers { get; set; }
	}
}