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
		
		public Color GetWeightedColor(Color start, Color end)
		{
			// help from http://jsfiddle.net/vksn3yLL/
			// via https://stackoverflow.com/questions/30143082/how-to-get-color-value-from-gradient-by-percentage-with-javascript

			var w = (ColorGradientPosition * 2) - 1;
			var w1 = (w / 1 + 1) / 2;
			var w2 = 1 - w1;

			return Color.FromArgb(
				Convert.ToByte(Math.Round(end.R * w1 + start.R * w2)),
				Convert.ToByte(Math.Round(end.G * w1 + start.G * w2)),
				Convert.ToByte(Math.Round(end.B * w1 + start.B * w2)));
		}
	}
}