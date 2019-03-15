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
		/// Position on color gradient based on estimate hours
		/// </summary>
		[NotMapped]
		public decimal ColorGradientPosition { get; set; }
		
		public ColorInfo GetColor(ColorInfo start, ColorInfo end)
		{						
			throw new NotImplementedException();			
		}

		public ColorInfo GetColor(string startColor, string endColor)
		{
			return GetColor(ColorInfo.FromHexValue(startColor), ColorInfo.FromHexValue(endColor));
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
	}
}