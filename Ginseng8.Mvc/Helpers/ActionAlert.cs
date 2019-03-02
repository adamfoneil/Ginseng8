using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Encodings.Web;

namespace Ginseng.Mvc.Helpers
{
	public static partial class HtmlHelpers
	{
		/// <summary>
		/// Conveys a success or error message in a Bootstrap alert div
		/// </summary>											 
		public static IHtmlContent ActionAlert<T>(this IHtmlHelper<T> html, ITempDataDictionary tempData)
		{
			foreach (string @class in AlertCss.AllMessageTypes)
			{
				if (tempData.ContainsKey(@class))
				{
					TagBuilder div = new TagBuilder("div");
					div.AddCssClass("alert");
					div.AddCssClass($"alert-{@class}");
					div.MergeAttribute("role", "alert");
					div.InnerHtml.Append(tempData[@class] as string);
					div.WriteTo(html.ViewContext.Writer, HtmlEncoder.Default);
					break;
				}
			}			

			return null;
		}
	}
}