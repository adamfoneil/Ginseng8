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
	}
}