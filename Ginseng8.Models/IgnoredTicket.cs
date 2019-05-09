using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
    /// <summary>
    /// Indicates that a Freshdesk ticket is not actionable as a dev item
    /// </summary>
    public class IgnoredTicket : BaseTable
    {
        /// <summary>
        /// Maps to Ginseng.Mvc.Models.Freshdesk.Ticket.Id
        /// </summary>
        [PrimaryKey]
        public long TicketId { get; set; }

        [References(typeof(Organization))]
        public int OrganizationId { get; set; }
    }
}