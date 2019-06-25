using Microsoft.AspNetCore.Mvc.Rendering;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class AllMilestonesResult
    {
        public int TeamId { get; set; }
        public int ApplicationId { get; set; }
        public int Value { get; set; }
        public string Text { get; set; }

        public SelectListItem ToSelectListItem()
        {
            return new SelectListItem() { Value = Value.ToString(), Text = Text };
        }
    }

    public class AllMilestones : Query<AllMilestonesResult>
    {
        public AllMilestones() : base(
            @"SELECT
                [ms].[TeamId], [ms].[ApplicationId], [ms].[Id] AS [Value], [ms].[Name] + ': ' + FORMAT([ms].[Date], 'ddd M/d') AS [Text]
            FROM
				[dbo].[Milestone] [ms]
                INNER JOIN [dbo].[Team] [t] ON [ms].[TeamId]=[t].[Id]                
			WHERE
				[t].[OrganizationId]=@orgId AND
				[ms].[Date]>DATEADD(d, -7, getdate())
			ORDER BY
				[Date]")
        {
        }

        public int OrgId { get; set; }
    }
}