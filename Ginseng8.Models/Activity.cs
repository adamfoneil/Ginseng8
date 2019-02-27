using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	/// <summary>
	/// Defines some type of work that someone can do on a work item
	/// </summary>
	public class Activity : BaseTable
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		public int OrganizationId { get; set; }

		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		[MaxLength(255)]
		public string Description { get; set; }

		/// <summary>
		/// Overall order of activity
		/// </summary>
		public int Order { get; set; }

		public bool IsActive { get; set; } = true;
	}
}