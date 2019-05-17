using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
    /// <summary>
    /// Indicates a responsibility ruling out action on a given Freshdesk ticket
    /// </summary>
    public class IgnoredTicket : BaseTable
    {
        /// <summary>
        /// Maps to Ginseng.Mvc.Models.Freshdesk.Ticket.Id
        /// </summary>
        [PrimaryKey]
        public long TicketId { get; set; }

        [References(typeof(Organization))]
        [PrimaryKey]        
        public int OrganizationId { get; set; }

        [References(typeof(Responsibility))]
        [PrimaryKey]
        public int ResponsibilityId { get; set; }

        public string Comments { get; set; }
    }
}