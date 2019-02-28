using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Ginseng.Mvc.TagHelpers
{
	// see https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/authoring?view=aspnetcore-2.2
	// thanks also to https://www.jerriepelser.com/blog/accessing-request-object-inside-tag-helper-aspnet-core/

	[HtmlTargetElement("activelink")]
	public class ActiveLink : TagHelper
	{
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName("page")]
		public string Page { get; set; }

		[HtmlAttributeName("text")]
		public string Text { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			base.Process(context, output);

			output.TagName = "a";
			output.Attributes.SetAttribute("href", Page);
			output.Content.SetContent(Text);
			output.AddClass("nav-link", HtmlEncoder.Default);

			string currentPage = ViewContext.RouteData.Values["page"] as string;
			if (currentPage.Equals(Page)) output.AddClass("active", HtmlEncoder.Default);

			output.TagMode = TagMode.StartTagAndEndTag;
		}
	}
}