using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class MyLabelSubscriptions : Query<LabelSubscription>
    {
        public MyLabelSubscriptions() : base(
            @"SELECT
                [ls].*
            FROM
                [dbo].[LabelSubscription] [ls]
            WHERE
                [UserId]=@userId AND
                [OrganizationId]=@orgId")
        {
        }

        public int UserId { get; set; }
        public int OrgId { get; set; }
    }
}