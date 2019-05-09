using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class AllWorkItemTickets : Query<WorkItemTicket>
    {
        public AllWorkItemTickets() : base("SELECT * FROM [dbo].[WorkItemTicket] WHERE [OrganizationId]=@orgId")
        {
        }
       
        public int OrgId { get; set; }

        [Case(true, "[WorkItemNumber]<=0")]
        public bool? IsIgnored { get; set; }
    }
}