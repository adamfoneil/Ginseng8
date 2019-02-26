using Ginseng8.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng8.Models
{
	public class OrganizationUser : BaseTable
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		public int OrganizationId { get; set; }

		[References(typeof(UserProfile))]
		[PrimaryKey]
		public int UserId { get; set; }

		[MaxLength(20)]
		public string DisplayName { get; set; }

		/// <summary>
		/// This is a join request
		/// </summary>
		public bool IsRequest { get; set; }

		/// <summary>
		/// User is allowed into the org (join request accepted)
		/// </summary>
		public bool IsEnabled { get; set; }
	}
}