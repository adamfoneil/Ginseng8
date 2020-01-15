using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class PendingWorkLogs : Query<PendingWorkLog>
    {
        public PendingWorkLogs() : base(
            @"SELECT [wl].*, [wi].[Number] AS [WorkItemNumber], [wi].[Title] AS [WorkItemTitle]
            FROM [dbo].[PendingWorkLog] [wl]
            LEFT JOIN [dbo].[WorkItem] [wi] ON [wl].[WorkItemId]=[wi].[Id]
            WHERE [wl].[OrganizationId]=@orgId {andWhere}")
        {
        }

        public int OrgId { get; set; }

        [Where("[wl].[ApplicationId]=@appId")]
        public int? AppId { get; set; }

        [Where("[wl].[UserId]=@userId")]
        public int? UserId { get; set; }
    }
}