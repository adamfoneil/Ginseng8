using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

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
		/// Relative numeric impression of this size (for example 1 = Small, 3 = Medium, 7 = Large)
		/// </summary>
		public int WorkDays { get; set; }
	}
}