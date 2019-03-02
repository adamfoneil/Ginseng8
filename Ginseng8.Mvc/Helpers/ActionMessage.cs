using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace Ginseng.Mvc.Helpers
{
	public static partial class HtmlHelpers
	{
		/// <summary>
		/// Conveys a success or error message in a Bootstrap alert div
		/// </summary>											 
		public static IHtmlContent ActionMessage<T>(this IHtmlHelper<T> html, ActionMessage message)
		{
			if (message == null) return null;

			TagBuilder div = new TagBuilder("div");
			div.AddCssClass("alert");
			div.AddCssClass(message.CssClass);
			div.InnerHtml.Append(message.Content);
			div.WriteTo(html.ViewContext.Writer, HtmlEncoder.Default);

			return null;
		}
	}
}