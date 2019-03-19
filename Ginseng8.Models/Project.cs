using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	/// <summary>
	/// A collection of work items centered around a single goal, feature, or some other unifying idea
	/// </summary>
	public class Project : BaseTable, IBody
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
		/// Indicates that the project applies to a specific app
		/// </summary>
		[References(typeof(Application))]
		public int? ApplicationId { get; set; }

		/// <summary>
		/// Overall priority of project (lower number = higher priority)
		/// </summary>
		public int? Priority { get; set; }

		[MaxLength(255)]
		public string BranchUrl { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		public bool IsActive { get; set; } = true;
	}
}