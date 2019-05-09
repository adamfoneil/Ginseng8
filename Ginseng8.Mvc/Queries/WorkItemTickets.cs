using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class WorkItemTickets : Query<WorkItemTicket>
    {
        public WorkItemTickets() : base(
            @"SELECT *
			FROM [dbo].[WorkItemTicket]			
			WHERE [OrganizationId] = @organizationId")
        {
        }

        public int OrganizationId { get; set; }

        [Where("[TicketId] = @ticketId")]
        public long? TicketId { get; set; }

        [Where("[WorkItemNumber] = @workItemNumber")]
        public int? WorkItemNumber { get; set; }

    }
}
