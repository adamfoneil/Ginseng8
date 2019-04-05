using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
	/// <summary>
	/// Describes a user's listening for hand-offs for a given app and activity.
	/// Provides a hook for showing hung tasks on My Items in activities I follow
	/// </summary>
	public class ActivitySubscription : BaseTable
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		public int OrganizationId { get; set; }

		[References(typeof(UserProfile))]
		[PrimaryKey]
		public int UserId { get; set; }

		[References(typeof(Application))]
		[PrimaryKey]
		public int ApplicationId { get; set; }

		[References(typeof(Activity))]
		[PrimaryKey]
		public int ActivityId { get; set; }

		/// <summary>
		/// Alert me by email when a hand-off is done for this activity and app
		/// </summary>
		public bool SendEmail { get; set; }

		/// <summary>
		/// Alert me by text when a hand-off is done for this activity and app
		/// </summary>
		public bool SendText { get; set; }

		/// <summary>
		/// Show notification within the site
		/// </summary>
		public bool InApp { get; set; }

		[NotMapped]
		public string AppName { get; set; }

		[NotMapped]
		public string ActivityName { get; set; }		
	}
}