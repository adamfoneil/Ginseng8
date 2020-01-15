using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
    public class MyPinnedItems : Query<int>
    {
        public MyPinnedItems() : base(
            @"SELECT [p].[WorkItemId] 
            FROM [dbo].[PinnedWorkItem] [p] 
            INNER JOIN [dbo].[WorkItem] [wi] ON [p].[WorkItemId]=[wi].[Id]
            WHERE [p].[UserId]=@userId AND [wi].[OrganizationId]=@orgId")
        {
        }

        public int OrgId { get; set; }
        public int UserId { get; set; }
    }
}
