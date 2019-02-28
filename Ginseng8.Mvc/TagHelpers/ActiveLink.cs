using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Ginseng.Mvc.TagHelpers
{
	// see https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/authoring?view=aspnetcore-2.2
	[HtmlTargetElement("a")]
	public class ActiveLink : AnchorTagHelper
	{
		public ActiveLink(IHtmlGenerator generator) : base(generator)
		{
		}

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			string currentPage = ViewContext.RouteData.Values["page"] as string;
			if (currentPage.Equals(Page)) output.AddClass("active", HtmlEncoder.Default);
			await base.ProcessAsync(context, output);
		}
	}
}