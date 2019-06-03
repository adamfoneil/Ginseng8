using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class IgnoredTickets : Query<long>
    {
        public IgnoredTickets() : base(
            @"SELECT [TicketId] 
            FROM [dbo].[IgnoredTicket] 
            WHERE [ResponsibilityId]=@responsibilityId AND [OrganizationId]=@orgId
            ORDER BY [DateCreated] DESC
            {offset}")
        {
        }

        public int ResponsibilityId { get; set; }
        public int OrgId { get; set; }

        [Offset(10)]
        public int? Page { get; set; }
    }
}