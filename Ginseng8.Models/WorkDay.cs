using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
	public class WorkDay : AppTable
	{
		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		[MaxLength(3)]
		public string Abbreviation { get; set; }

		/// <summary>
		/// SQL Server DATEPART(dw...) value
		/// </summary>
		public int Value { get; set; }

		/// <summary>
		/// Bit mask value
		/// </summary>
		public int Flag { get; set; }

		[NotMapped]
		public bool IsSelected { get; set; }
	}
}