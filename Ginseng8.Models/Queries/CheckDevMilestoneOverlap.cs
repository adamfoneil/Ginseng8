using Postulate.Base;
using System;

namespace Ginseng.Models.Queries
{
    public class CheckDevMilestoneOverlapResult
    {
        public int DeveloperId { get; set; }
        public int MilestoneId { get; set; }
        public string MilestoneName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class CheckDevMilestoneOverlap : Query<CheckDevMilestoneOverlapResult>
    {
        public CheckDevMilestoneOverlap() : base(
            @"WITH [committed] AS (
                SELECT
                    [dm].[DeveloperId],
                    [dm].[MilestoneId],
                    [ms].[Name] AS [MilestoneName],
                    [dm].[StartDate],
                    [ms].[Date] AS [EndDate]
                FROM
                    [dbo].[DeveloperMilestone] [dm]
                    INNER JOIN [dbo].[Milestone] [ms] ON [dm].[MilestoneId]=[ms].[Id]
                WHERE
                    [dm].[DeveloperId]=@userId
            ) SELECT
                [c].*
            FROM
                [committed] [c]
            WHERE
                @checkDate BETWEEN [StartDate] AND [EndDate]")
        {
        }

        public int UserId { get; set; }
        public DateTime CheckDate { get; set; }
    }
}