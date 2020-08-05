using Dapper.QX.Abstract;
using Dapper.QX.Interfaces;
using System;
using System.Collections.Generic;

namespace Ginseng.Mvc.Queries
{
    public class InitMilestoneView : TestableQuery<int>
    {
        public InitMilestoneView() : base(
            @"INSERT INTO [dbo].[MilestoneUserView] (
                [MilestoneId], [UserId], [IsVisible], [DateCreated], [CreatedBy]
            ) SELECT 
                [ms].[Id], @userId, 1, @localDate, @userName
            FROM 
                [dbo].[Milestone] [ms]
            WHERE 
                [ms].[OrganizationId]=@orgId AND 
                NOT EXISTS(SELECT 1 FROM [dbo].[MilestoneUserView] WHERE [MilestoneId]=[ms].[Id] AND [UserId]=@userId);

            INSERT INTO [dbo].[MilestoneUserView] (
                [MilestoneId], [UserId], [IsVisible], [DateCreated], [CreatedBy]
            ) SELECT 
                0, @userId, 1, @localDate, @userName
            FROM
                [dbo].[FnIntRange](0, 0)
            WHERE
                NOT EXISTS(SELECT 1 FROM [dbo].[MilestoneUserView] WHERE [MilestoneId]=0 AND [UserId]=@userId)")
        {
        }

        public int OrgId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime LocalDate { get; set; }

        protected override IEnumerable<ITestableQuery> GetTestCasesInner()
        {
            yield return new InitMilestoneView() { UserId = 10, UserName = "adamosoftware@gmail.com", LocalDate = DateTime.Now, OrgId = 1 };
        }
    }
}
