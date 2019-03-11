using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Helpers
{
	public static partial class HtmlHelpers
	{
		public const string AllApps = "(all apps)";

		public static IHtmlContent CurrentAppName(this IHtmlHelper<dynamic> html)
		{
			return new HtmlString(CurrentAppNameString(html));
		}

		public static string CurrentAppNameString(this IHtmlHelper<dynamic> html)
		{
			AppPageModel model = html.ViewContext.ViewData.Model as AppPageModel;			
			return (model != null && model.CurrentOrgUser.CurrentApp != null) ? model.CurrentOrgUser.CurrentApp.Name : AllApps;
		}

		public static async Task<SelectList> AppFilterOptions(this IHtmlHelper<dynamic> html)
		{
			AppPageModel model = html.ViewContext.ViewData.Model as AppPageModel;
			if (model != null)
			{
				return await model.CurrentOrgAppSelectAsync();
			}

			return null;
		}
	}
}