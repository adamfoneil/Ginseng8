using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class TeamLabelsInUseByLabel : Query<Team>
    {
        public TeamLabelsInUseByLabel() : base(
            @"SELECT
                [tl].[LabelId],
                [t].*
            FROM
                [dbo].[Team] [t]
                INNER JOIN [dbo].[TeamLabel] [tl] ON [t].[Id]=[tl].[TeamId]
            WHERE
                [t].[OrganizationId]=@orgId {andWhere}")
        {
        }

        public int OrgId { get; set; }

        [Where("[tl].[LabelId]=@labelId")]
        public int? LabelId { get; set; }
    }
}