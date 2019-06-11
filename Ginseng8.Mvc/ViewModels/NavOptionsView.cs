using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
	public class NavOptionsView
	{
		public string Text { get; set; }        
        public string PluralText { get; set; }
        public string UpdateAction { get; set; }
		public IEnumerable<SelectListItem> Options { get; set; }
	}
}