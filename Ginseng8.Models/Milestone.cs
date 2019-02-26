using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	/// <summary>
	/// Some point in time or a named stopping point when tasks can be due
	/// </summary>
	public class Milestone : BaseTable
	{
		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }

		public DateTime? DueDate { get; set; }

		public bool IsActive { get; set; }
	}
}