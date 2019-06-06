using Ginseng.Models;
using Postulate.Base;

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
                [app].[OrganizationId]=@orgId")
        {
        }

        public int OrgId { get; set; }
    }
}