using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class DevMilestones : Query<DeveloperMilestone>
    {
        public DevMilestones() : base(
            @"SELECT [dm].* 
            FROM [dbo].[DeveloperMilestone] [dm] 
            INNER JOIN [dbo].[OrganizationUser] [ou] ON [dm].[DeveloperId]=[ou].[UserId]
            WHERE [ou].[OrganizationId]=@orgId AND [dm].[MilestoneId]=@milestoneId")
        {
        }

        public int OrgId { get; set; }

        public int MilestoneId { get; set; }
    }
}
