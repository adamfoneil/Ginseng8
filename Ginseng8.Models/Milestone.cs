using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
	/// <summary>
	/// A due date of some kind, such as a sprint end date or some other event with a known date
	/// </summary>
	public class Milestone : BaseTable
	{
		[PrimaryKey]
		[References(typeof(Organization))]
		public int OrganizationId { get; set; }

		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }

		[Column(TypeName = "date")]
		[DisplayFormat(DataFormatString = "{0:M/d/yy}")]
		public DateTime Date { get; set; }
	}
}