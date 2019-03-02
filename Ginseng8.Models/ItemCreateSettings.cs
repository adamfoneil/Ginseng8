using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
	public class ItemCreateSettings : BaseTable
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		public int OrganizationId { get; set; }

		[References(typeof(UserProfile))]
		[PrimaryKey]
		public int UserId { get; set; }

		/// <summary>
		/// App selected by default on item creation form
		/// </summary>
		[References(typeof(Application))]
		public int? ApplicationId { get; set; }

		/// <summary>
		/// Project selected by default
		/// </summary>
		[References(typeof(Project))]
		public int? ProjectId { get; set; }
	}
}