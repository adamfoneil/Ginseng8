using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Extensions
{
	/// <summary>
	/// Thanks to https://stackoverflow.com/a/54043063/2023653 and
	/// https://www.learnrazorpages.com/advanced/render-partial-to-string
	/// </summary>
	public static class PageExtensions
	{
		public static async Task<string> RenderViewAsync(this PageModel pageModel, string pageName)
		{
			var actionContext = new ActionContext(
				new DefaultHttpContext(),
				new RouteData(),
				new ActionDescriptor()
			);

			using (var stringWriter = new StringWriter())
			{
				IRazorViewEngine razorViewEngine = pageModel.HttpContext.RequestServices.GetService(typeof(IRazorViewEngine)) as IRazorViewEngine;
				IRazorPageActivator activator = pageModel.HttpContext.RequestServices.GetService(typeof(IRazorPageActivator)) as IRazorPageActivator;

				var result = razorViewEngine.FindPage(actionContext, pageName);

				if (result.Page == null) throw new ArgumentNullException($"The page {pageName} cannot be found.");

				var page = result.Page;

				var view = new RazorView(razorViewEngine,
					activator,
					new List<IRazorPage>(),
					page,
					HtmlEncoder.Default,
					new DiagnosticListener("ViewRenderService"));

				var viewContext = new ViewContext(
					actionContext,
					view,
					pageModel.ViewData,
					pageModel.TempData,
					stringWriter,
					new HtmlHelperOptions()
				);

				var pageNormal = (Page)result.Page;

				pageNormal.PageContext = pageModel.PageContext;

				pageNormal.ViewContext = viewContext;

				activator.Activate(pageNormal, viewContext);

				await page.ExecuteAsync();

				return stringWriter.ToString();
			}
		}
	}
}
