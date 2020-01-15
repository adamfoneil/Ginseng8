using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class NewItemAppLabelsInUse : Query<Application>
    {
        public NewItemAppLabelsInUse() : base(
            @"SELECT
                [nal].[LabelId],
                [app].*
            FROM
                [dbo].[Application] [app]
                INNER JOIN [dbo].[NewItemAppLabel] [nal] ON [app].[Id]=[nal].[ApplicationId]
            WHERE
                [app].[OrganizationId]=@orgId {andWhere}")
        {
        }

        public int OrgId { get; set; }

        [Where("[nal].[LabelId]=@labelId")]
        public int? LabelId { get; set; }
    }
}