using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class AllIgnoredTickets : Query<IgnoredTicket>
    {
        public AllIgnoredTickets() : base("SELECT * FROM [dbo].[IgnoredTicket] WHERE [OrganizationId]=@orgId")
        {
        }

        public int OrgId { get; set; }
    }
}