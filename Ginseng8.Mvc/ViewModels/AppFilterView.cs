using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class AppFilterView
	{
		public string CurrentAppName { get; set; }
		public IEnumerable<SelectListItem> AppOptions { get; set; }
	}
}