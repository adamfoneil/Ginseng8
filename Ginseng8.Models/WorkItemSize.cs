using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;

namespace Ginseng.Models
{
	/// <summary>
	/// Broad category of task estimate size (for example, Small, Medium, Large)
	/// </summary>
	public class WorkItemSize : BaseTable
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		public int OrganizationId { get; set; }

		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		/// <summary>
		/// What characterizes tasks of this size?
		/// </summary>
		[MaxLength(255)]
		public string Description { get; set; }

		/// <summary>
		/// Number of hours expected for this work item
		/// </summary>
		public int EstimateHours { get; set; }

		/// <summary>
		/// Position on color gradient based on estimate hours (between 0 and 1)
		/// </summary>
		[NotMapped]
		public decimal ColorGradientPosition { get; set; }
		
		public ColorInfo GetWeightedColor(ColorInfo start, ColorInfo end)
		{
			// help from http://jsfiddle.net/vksn3yLL/
			// via https://stackoverflow.com/questions/30143082/how-to-get-color-value-from-gradient-by-percentage-with-javascript

			var w = (ColorGradientPosition * 2) - 1;
			var w1 = (w / 1 + 1) / 2;
			var w2 = 1 - w1;

			return new ColorInfo()
			{
				Red = Convert.ToInt32(Math.Round(end.Red * w1 + start.Red * w2)),
				Green = Convert.ToInt32(Math.Round(end.Green * w1 + start.Green * w2)),
				Blue = Convert.ToInt32(Math.Round(end.Blue * w1 + start.Blue * w2))
			};
		}

		public ColorInfo GetWeightedColor(string startColor, string endColor)
		{
			return GetWeightedColor(ColorInfo.FromHexValue(startColor), ColorInfo.FromHexValue(endColor));
		}
	}

	public struct ColorInfo
	{
		public int Red { get; set; }
		public int Green { get; set; }
		public int Blue { get; set; }

		public static ColorInfo FromHexValue(string colorValue)
		{
			int[] values = Chunk(colorValue.Substring(1), 2).Select(hex => Convert.ToInt32(hex, 16)).ToArray();
			return new ColorInfo() { Red = values[0], Green = values[1], Blue = values[2] };
		}

		private static IEnumerable<string> Chunk(string input, int length)
		{
			// thanks to https://stackoverflow.com/a/1450797/2023653
			return Enumerable.Range(0, input.Length / length).Select(i => input.Substring(i * length, length));
		}

		public string ToHex()
		{
			string Hex(int value)
			{
				return value.ToString("X");
			};

			return $"#{Hex(Red)}{Hex(Green)}{Hex(Blue)}";
		}
	}
}