using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ginseng.Mvc.Helpers
{
	public static partial class HtmlHelpers
	{
		public const string AppName = "Ginseng8";

		public static IHtmlContent CurrentOrgName(this IHtmlHelper<dynamic> html)
		{
			string result = AppName;

			AppPageModel model = html.ViewContext.ViewData.Model as AppPageModel;
			if (model != null) result = model.OrgName;

			return new HtmlString(result);
		}
	}
}