using Ginseng.Models.Conventions;
using Ginseng.Models.Enums.Freshdesk;
using Postulate.Base.Attributes;
using System;

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

        public TicketType TicketType { get; set; }

		/// <summary>
		/// Maps to Ginseng.Mvc.Models.Freshdesk.Ticket.Status
		/// </summary>
		public TicketStatus TicketStatus { get; set; }

		public DateTime? TicketStatusDateModified { get; set; }

		[References(typeof(Organization))]
		public int OrganizationId { get; set; }

		/// <summary>
		/// This should reflect in Freshdesk ticket custom field.
        /// Use zero to mean ticket is ignored (test ticket, or not actionable by dev team)
		/// </summary>
		public int WorkItemNumber { get; set; }
	}

	/*
	events to handle:

	- WorkItemTicket created: 
		Set custom field on corresponding Freshdesk ticket so users can see Ginseng work item and its status.
		The custom field should be a link like this: <a href="https://ginseng8.azurewebsites.net/WorkItem/View/{number}">{number} {status}</a>
		[pseudocode] status = (WorkItem.CloseReasonId == null) ? "Open" : CloseReason.Name;

	- WorkItem CloseReasonId changes:
		Update the ticket custom field with link with reflecting new status

	- Freshdesk ticket status changes:
		Update WorkItemTicket.TicketStatus and TicketStatusDateModified with UTC datetime
	*/
}