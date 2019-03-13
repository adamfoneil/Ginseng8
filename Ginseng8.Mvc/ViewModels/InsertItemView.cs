using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.ViewModels
{
	public class InsertItemView
	{
		private readonly Dictionary<string, int> _contextValues;

		public InsertItemView(Dictionary<string, int> contextValues)
		{			
			_contextValues = contextValues;
		}

		public string Context
		{
			get
			{
				return string.Join("-", _contextValues.Select(kp => $"{kp.Key}-{kp.Value}"));
			}
		}

		public IHtmlContent WriteContextFields(IHtmlHelper html)
		{			
			// filter out zeroes from fields because they are FK violations
			foreach (var kp in _contextValues.Where(kp => kp.Value != 0))
			{
				TagBuilder input = new TagBuilder("input");
				input.MergeAttribute("type", "hidden");
				input.MergeAttribute("name", kp.Key);
				input.MergeAttribute("value", kp.Value.ToString());
				html.ViewContext.Writer.Write(input);				
			}

			var returnUrl = new TagBuilder("input");
			returnUrl.MergeAttribute("type", "hidden");
			returnUrl.MergeAttribute("name", "returnUrl");
			returnUrl.MergeAttribute("value", UriHelper.GetDisplayUrl(html.ViewContext.HttpContext.Request));
			html.ViewContext.Writer.Write(returnUrl);

			return null;
		}
	}
}