using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class IgnoredTickets : Query<long>
    {
        public IgnoredTickets() : base(
            @"SELECT [TicketId] FROM [dbo].[IgnoredTicket] 
            WHERE [ResponsibilityId]=@responsibilityId AND [OrganizationId]=@orgId")
        {
        }

        public int ResponsibilityId { get; set; }
        public int OrgId { get; set; }
    }
}