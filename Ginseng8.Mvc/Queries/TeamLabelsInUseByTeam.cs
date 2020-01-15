using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class TeamLabelsInUseByTeam : Query<Label>
    {
        public TeamLabelsInUseByTeam() : base(
            @"SELECT
                [tl].[TeamId],
                [lbl].*
            FROM
                [dbo].[Label] [lbl]
                INNER JOIN [dbo].[TeamLabel] [tl] ON [lbl].[Id]=[tl].[LabelId]
            WHERE
                [lbl].[OrganizationId]=@orgId {andWhere}")
        {
        }

        public int OrgId { get; set; }

        [Where("[tl].[TeamId]=@teamId")]
        public int? TeamId { get; set; }
    }
}