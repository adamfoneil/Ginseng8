using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Extensions
{
	/// <summary>
	/// thanks to https://stackoverflow.com/a/50024209/2023653
	/// also https://long2know.com/2017/08/rendering-and-emailing-embedded-razor-views-with-net-core/
	/// </summary>
	public static class ControllerExtensions
	{
		public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
		{
			if (string.IsNullOrEmpty(viewName))
			{
				viewName = controller.ControllerContext.ActionDescriptor.ActionName;
			}

			controller.ViewData.Model = model;

			using (var writer = new StringWriter())
			{
				IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
				ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

				if (!viewResult.Success) throw new Exception($"A view with the name {viewName} could not be found");

				ViewContext viewContext = new ViewContext(
					controller.ControllerContext,
					viewResult.View,
					controller.ViewData,
					controller.TempData,
					writer,
					new HtmlHelperOptions()
				);

				await viewResult.View.RenderAsync(viewContext);

				return writer.GetStringBuilder().ToString();
			}
		}
	}
}