using Humanizer;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Ginseng.Mvc.Helpers
{
    public static partial class HtmlHelpers
    {
        public static string ElapsedTime<T>(this IHtmlHelper<T> html, DateTime then, DateTime now)
        {
            return now.Subtract(then).Humanize();
        }

        public static bool HasNotifications<T>(this IHtmlHelper<T> html)
        {
            var model = html.ViewContext.ViewData.Model as AppPageModel;
            return (model != null) ? model.HasNotifications : false;
        }
    }
}
