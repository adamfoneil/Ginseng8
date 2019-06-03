using Microsoft.AspNetCore.Mvc.Rendering;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class AllMilestonesResult
    {
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
                [ms].[ApplicationId], [ms].[Id] AS [Value], [ms].[Name] + ': ' + FORMAT([ms].[Date], 'ddd M/d') AS [Text]
            FROM
				[dbo].[Milestone] [ms]
                INNER JOIN [dbo].[Application] [app] ON [ms].[ApplicationId]=[app].[Id]
			WHERE
				[app].[OrganizationId]=@orgId AND
				[ms].[Date]>DATEADD(d, -7, getdate())
			ORDER BY
				[Date]")
        {
        }

        public int OrgId { get; set; }
    }
}