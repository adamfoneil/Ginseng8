using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Drawing;

namespace Ginseng.Mvc.Helpers
{
	public static partial class HtmlHelpers
	{
		// gradient colors from https://uigradients.com/#JShine (with the middle color removed -- I support only two gradient colors)

		// for work item size color "temperature" gradient
		public static Color StartColor
		{
			get
			{
				var converter = new ColorConverter();				
				return (Color)converter.ConvertFromString("#12c2e9");				
			}
		}

		public static Color EndColor
		{
			get
			{
				var converter = new ColorConverter();
				return (Color)converter.ConvertFromString("#f64f59");
			}
		}

		public static string ColorToHex(this IHtmlHelper html, Color color)
		{
			// thanks to
			// https://gunnarpeipman.com/net/color-to-hex/
			// https://stackoverflow.com/a/48176425/2023653

			return ColorTranslator.ToHtml(color);
		}

		public static string WeightedColorToHex(this IHtmlHelper html, decimal position)
		{
			return WeightedColorToHex(html, StartColor, EndColor, position);
		}

		public static string WeightedColorToHex(this IHtmlHelper html, Color startGradient, Color endGradient, decimal position)
		{
			return ColorToHex(html, WeightedColor(startGradient, endGradient, position));
		}

        public static string WeightedColorToHex(decimal position)
        {
            var color = WeightedColor(StartColor, EndColor, position);
            return ColorTranslator.ToHtml(color);
        }

		public static Color WeightedColor(Color startGradient, Color endGradient, decimal position)
		{
			// help from http://jsfiddle.net/vksn3yLL/
			// via https://stackoverflow.com/questions/30143082/how-to-get-color-value-from-gradient-by-percentage-with-javascript

			var w = (position * 2) - 1;
			var w1 = (w / 1 + 1) / 2;
			var w2 = 1 - w1;

			return Color.FromArgb(
				Convert.ToByte(Math.Round(endGradient.R * w1 + startGradient.R * w2)),
				Convert.ToByte(Math.Round(endGradient.G * w1 + startGradient.G * w2)),
				Convert.ToByte(Math.Round(endGradient.B * w1 + startGradient.B * w2)));
		}
	}
}