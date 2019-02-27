﻿using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	/// <summary>
	/// A descriptor such as Bug or Feature or some other wording that helps categorize work items
	/// </summary>
	public class Label : BaseTable
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		public int OrganizationId { get; set; }

		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }

		[MaxLength(50)]
		public string CssClass { get; set; }

		[MaxLength(50)]
		public string BackColor { get; set; }

		[MaxLength(50)]
		public string ForeColor { get; set; }

		public bool IsActive { get; set; } = true;
	}
}