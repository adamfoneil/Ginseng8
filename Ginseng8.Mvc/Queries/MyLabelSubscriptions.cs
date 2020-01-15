using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class MyLabelSubscriptions : Query<LabelSubscription>
    {
        public MyLabelSubscriptions() : base(
            @"SELECT
                [ls].*,
                [lbl].[Name] AS [LabelName],
                COALESCE([app].[Name], '(all applications)') AS [ApplicationName]
            FROM
                [dbo].[LabelSubscription] [ls]
                INNER JOIN [dbo].[Label] [lbl] ON [ls].[LabelId]=[lbl].[Id]
                LEFT JOIN [dbo].[Application] [app] ON [ls].[ApplicationId]=[app].[Id]
            WHERE
                [ls].[UserId]=@userId AND                
                [ls].[OrganizationId]=@orgId {andWhere}")
        {
        }

        public int UserId { get; set; }
        public int OrgId { get; set; }

        [Where("[ls].[ApplicationId]=@appId")]
        public int? AppId { get; set; }

        [Where("[ls].[InApp]=@inApp")]
        public bool? InApp { get; set; }
    }
}