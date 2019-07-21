using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class TeamLabels : Query<Label>
    {
        public TeamLabels() : base(
            @"SELECT
                [tl].[TeamId],
                [lbl].*
            FROM
                [dbo].[TeamLabel] [tl]
                INNER JOIN [dbo].[Label] [lbl] ON [tl].[LabelId]=[lbl].[Id]
            WHERE
                [lbl].[OrganizationId]=@orgId AND
                [tl].[TeamId]=@teamId")
        {
        }

        public int OrgId { get; set; }
        public int TeamId { get; set; }
    }
}