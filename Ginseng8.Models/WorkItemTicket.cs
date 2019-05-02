using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
	/// <summary>
	/// Associates a work item with a Freshdesk ticket.
	/// Note that many tickets may link to a single work item, but I don't think the reverse would be practical.
	/// Therefore, the primary key is on the Freshdesk side.
	/// This association should reflect in a ticket custom field
	/// </summary>
	public class WorkItemTicket : BaseTable
	{
		/// <summary>
		/// Maps to Ginseng.Mvc.Models.Freshdesk.Ticket.Id
		/// </summary>
		[PrimaryKey]
		public long TicketId { get; set; }

		[References(typeof(Organization))]
		public int OrganizationId { get; set; }

		/// <summary>
		/// This should reflect in Freshdesk ticket custom field.
		/// </summary>
		public int WorkItemNumber { get; set; }
	}
}