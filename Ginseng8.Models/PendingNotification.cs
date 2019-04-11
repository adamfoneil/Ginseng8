using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	public enum TransportMethod
	{
		Email,
		Text,
		App
	}

	[Identity(nameof(Id))]
	public class PendingNotification
	{
		public int Id { get; set; }

		[References(typeof(EventLog))]
		public int EventLogId { get; set; }

		public TransportMethod Method { get; set; }

		[MaxLength(255)]
		[Required]
		public string SendTo { get; set; }
	}
}