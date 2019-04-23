using Ginseng.Models.Queries;
using Postulate.Base.Attributes;
using Postulate.SqlServer.IntKey;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	public enum DeliveryMethod
	{
		Email = 1,
		Text = 2,
		App = 3
	}	

	/// <summary>
	/// Holds pending and delivered notifications. This is queried through a call from cron-job.org
	/// every 10 minutes or whatever that sends notifications in batches of 50 or some such.
	/// </summary>
	[Identity(nameof(Id))]
	public class Notification
	{
		public int Id { get; set; }

		[References(typeof(EventLog))]
		public int EventLogId { get; set; }

		/// <summary>
		/// User's local time
		/// </summary>
		public DateTime DateCreated { get; set; }		

		public DeliveryMethod Method { get; set; }

		[MaxLength(255)]
		[Required]
		public string SendTo { get; set; }

		[Required]
		public string Content { get; set; }

		/// <summary>
		/// EventSubscription.Id or ActivitySubscription.Id (used for unsubscribe link)
		/// </summary>
		public int SourceId { get; set; }
		
		/// <summary>
		/// EventSubscription or ActivitySubscription (needed for unsubscribe link)
		/// </summary>
		[Required]
		[MaxLength(50)]
		public string SourceTable { get; set; }

		/// <summary>
		/// Indicates when message was delivered. If null, then it's still pending
		/// </summary>
		public DateTime? DateDelivered { get; set; }

		public static async Task CreateFromEventSubscriptions(IDbConnection connection, int eventLogId)
		{
			await new InsertEventSubscriptionEmailNotifications() { Id = eventLogId }.ExecuteAsync(connection);
			await new InsertEventSubscriptionTextNotifications() { Id = eventLogId }.ExecuteAsync(connection);
			// todo: app notifications
		}

		public static async Task CreateFromActivitySubscriptions(IDbConnection connection, int eventLogId)
		{
			await new InsertActivitySubscriptionEmailNotifications() { Id = eventLogId }.ExecuteAsync(connection);
			await new InsertActivitySubscriptionTextNotifications() { Id = eventLogId }.ExecuteAsync(connection);
			// todo: app notifications
		}

		internal static async Task CreateFromMentionAsync(IDbConnection connection, Comment comment, OrganizationUser orgUser)
		{
			throw new NotImplementedException();
		}
	}
}