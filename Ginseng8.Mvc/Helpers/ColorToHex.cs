using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing;

namespace Ginseng.Mvc.Helpers
{
	public static partial class HtmlHelpers
	{
		public static string ColorToHex(this IHtmlHelper html, Color color)
		{
			// thanks to
			// https://gunnarpeipman.com/net/color-to-hex/
			// https://stackoverflow.com/a/48176425/2023653

			return ColorTranslator.ToHtml(color);
		}
	}
}