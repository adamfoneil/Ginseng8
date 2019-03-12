using Microsoft.AspNetCore.Html;
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
			foreach (var kp in _contextValues)
			{
				TagBuilder input = new TagBuilder("input");
				input.MergeAttribute("type", "hidden");
				input.MergeAttribute("name", kp.Key);
				input.MergeAttribute("value", kp.Value.ToString());
				html.ViewContext.Writer.Write(input);				
			}
			return null;
		}
	}
}