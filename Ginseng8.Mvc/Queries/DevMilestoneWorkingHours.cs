using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class DevMilestoneWorkingHoursResult
    {
        public int ApplicationId { get; set; }
        public int MilestoneId { get; set; }
        public int DeveloperId { get; set; }
        public int TotalHours { get; set; }
    }

    public class DevMilestoneWorkingHours : Query<DevMilestoneWorkingHoursResult>
    {
        public DevMilestoneWorkingHours() : base(
            @"SELECT
                [ms].[ApplicationId],
                [dm].[MilestoneId],
                [dm].[DeveloperId], 
                SUM([wd].[Hours]) AS [TotalHours]
            FROM 
                [dbo].[DeveloperMilestone] [dm]
                INNER JOIN [dbo].[Milestone] [ms] ON [dm].[MilestoneId]=[ms].[Id]
                INNER JOIN [dbo].[Application] [app] ON [ms].[ApplicationId]=[app].[Id]
                CROSS APPLY [dbo].[FnWorkingDays](@orgId, [dm].[StartDate], [ms].[Date]) [wd]
            WHERE 
                [wd].[UserId]=[dm].[DeveloperId] AND
                [app].[OrganizationId]=@orgId {andWhere}
            GROUP BY
                [ms].[ApplicationId],
                [dm].[MilestoneId],
                [dm].[DeveloperId]")
        {
        }

        public int OrgId { get; set; }

        [Where("[dm].[MilestoneId]=@milestoneId")]
        public int? MilestoneId { get; set; }
    }
}
