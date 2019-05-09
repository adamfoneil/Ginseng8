using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class AllWorkItemTickets : Query<WorkItemTicket>
    {
        public AllWorkItemTickets() : base("SELECT * FROM [dbo].[WorkItemTicket] WHERE [OrganizationId]=@orgId")
        {
        }
       
        public int OrgId { get; set; }
    }
}