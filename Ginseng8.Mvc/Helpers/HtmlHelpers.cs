using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace Ginseng.Mvc.Helpers
{
	public static class HtmlHelpers
	{
		public static string CurrentPage(this IHtmlHelper<dynamic> html)
		{
			return html.ViewContext.RouteData.Values["page"] as string;
		}

		public static IHtmlContent ActiveLink(this IHtmlHelper<dynamic> html, string href, string text)
		{
			TagBuilder a = new TagBuilder("a");
			a.MergeAttribute("href", href);
			a.AddCssClass("nav-link");

			if (CurrentPage(html).Equals(href)) a.AddCssClass("active");
			a.WriteTo(html.ViewContext.Writer, HtmlEncoder.Default);

			return null;
		}
	}
}