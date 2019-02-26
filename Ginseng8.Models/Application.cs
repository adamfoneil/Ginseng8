using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	/// <summary>
	/// A software product managed by an organization
	/// </summary>
	public class Application : BaseTable
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		public int OrganizationId { get; set; }

		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		/// <summary>
		/// App's live URL, as applicable
		/// </summary>
		[MaxLength(255)]
		public string Url { get; set; }

		public bool IsActive { get; set; } = true;
	}
}