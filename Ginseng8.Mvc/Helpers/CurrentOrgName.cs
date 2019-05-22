using Ginseng.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.Helpers
{
	public static partial class HtmlHelpers
	{
		public const string AppName = "Ginseng";

		public static IHtmlContent CurrentOrgName(this IHtmlHelper<dynamic> html)
		{		
			return new HtmlString(CurrentOrgNameString(html));
		}

        public static int CurrentOrgId(this IHtmlHelper<dynamic> html)
        {
            AppPageModel model = html.ViewContext.ViewData.Model as AppPageModel;
            return (model != null) ? model.OrgId : 0;
        }

        public static bool CurrentOrgUsesFreshdesk(this IHtmlHelper<dynamic> html)
        {
            AppPageModel model = html.ViewContext.ViewData.Model as AppPageModel;
            return (model != null) ? model.CurrentOrg.UseFreshdesk() : false;
        }

		public static string CurrentOrgNameString(this IHtmlHelper<dynamic> html)
		{
			AppPageModel model = html.ViewContext.ViewData.Model as AppPageModel;
			return (model != null) ? model.OrgName : AppName;
		}

		public static IEnumerable<Organization> MySwitchOrgs(this IHtmlHelper<dynamic> html)
		{
			AppPageModel model = html.ViewContext.ViewData.Model as AppPageModel;
			return (model != null) ? model.SwitchOrgs : Enumerable.Empty<Organization>();
		}
	}
}