using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	/// <summary>
	/// Groups projects into levels of priority for easy comprehension of priorities on Dashboard/Projects
	/// </summary>
	public class PriorityTier : BaseTable
	{
		[PrimaryKey]
		[References(typeof(Organization))]
		public int OrganizationId { get; set; }

		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		/// <summary>
		/// The lowest priority tier always ignores this value, so it doesn't matter what you enter for the bottom tier
		/// </summary>
		public int MaxProjects { get; set; }

		/// <summary>
		/// Tier's sorted position
		/// </summary>
		public int Rank { get; set; }
	}
}