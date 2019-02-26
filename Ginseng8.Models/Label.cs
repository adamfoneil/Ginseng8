using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	/// <summary>
	/// A descriptor such as Bug or Feature or some other wording that helps clarify what a task is about
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

		public bool IsActive { get; set; } = true;
	}
}