using Dapper.QX;
using Dapper.QX.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class IgnoredTickets : Query<long>
    {
        public IgnoredTickets() : base(
            @"SELECT [TicketId] 
            FROM [dbo].[IgnoredTicket] 
            WHERE [TeamId]=@teamId AND [OrganizationId]=@orgId
            ORDER BY [DateCreated] DESC
            {offset}")
        {
        }

        public int TeamId { get; set; }
        public int OrgId { get; set; }

        [Offset(10)]
        public int? Page { get; set; }
    }
}