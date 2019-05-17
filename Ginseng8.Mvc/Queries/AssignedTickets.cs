using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class AssignedTickets : Query<long>
    {
        public AssignedTickets() : base("SELECT [TicketId] FROM [dbo].[WorkItemTicket] WHERE [OrganizationId]=@orgId")
        {
        }

        public int OrgId { get; set; }
    }
}