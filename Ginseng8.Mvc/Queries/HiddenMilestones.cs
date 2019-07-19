using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class HiddenMilestones: Query<Milestone>
    {
        public HiddenMilestones() : base(
            @"SELECT
	            [ms].[Id],
	            [ms].[Name],
	            [ms].[Date],
	            [ms].[CreatedBy],
	            [ms].[DateCreated],
	            [ms].[ModifiedBy],
	            [ms].[DateModified],
	            [ms].[TeamId],
	            [ms].[OrganizationId]
            FROM
	            [dbo].[Milestone] [ms]
	            INNER JOIN [dbo].[MilestoneUserView] [muv] ON [ms].[Id]=[muv].[MilestoneId]
            WHERE
	            [muv].[UserId]=@userId AND
	            [ms].[OrganizationId]=@orgId AND
	            [muv].[IsVisible]=0
            UNION
            SELECT
	            0, 
	            '(no milestone)' AS [Name],
	            NULL AS [Date],
	            [muv].[CreatedBy],
	            [muv].[DateCreated],
	            [muv].[ModifiedBy],
	            [muv].[DateModified],
	            NULL AS [TeamId],
	            @orgId AS [OrganizationId]
            FROM
	            [dbo].[MilestoneUserView] [muv]
            WHERE
	            [muv].[UserId]=@userId AND
	            [muv].[MilestoneId]=0 AND
	            [muv].[IsVisible]=0")
        {
        }

        public int OrgId { get; set; }
        public int UserId { get; set; }
    }
}
