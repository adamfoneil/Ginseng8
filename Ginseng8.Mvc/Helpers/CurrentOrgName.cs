using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ginseng.Mvc.Helpers
{
	public static partial class HtmlHelpers
	{
		public const string AppName = "Ginseng8";

		public static IHtmlContent CurrentOrgName(this IHtmlHelper<dynamic> html)
		{		
			return new HtmlString(CurrentOrgNameString(html));
		}

		public static string CurrentOrgNameString(this IHtmlHelper<dynamic> html)
		{
			AppPageModel model = html.ViewContext.ViewData.Model as AppPageModel;
			return (model != null) ? model.OrgName : AppName;
		}
	}
}