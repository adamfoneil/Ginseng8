using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class WorkItemTickets : Query<WorkItemTicket>
    {
        public WorkItemTickets() : base(
            @"SELECT *
			FROM [dbo].[WorkItemTicket]			
			WHERE [OrganizationId] = @organizationId {andWhere}")
        {
        }

        public int OrganizationId { get; set; }

        [Where("[TicketId] = @ticketId")]
        public long? TicketId { get; set; }

        [Where("[WorkItemNumber] = @workItemNumber")]
        public int? WorkItemNumber { get; set; }

    }
}
