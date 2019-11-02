using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class MyHandOffActivities : Query<ActivitySubscription>
	{
		public MyHandOffActivities() : base(
			@"SELECT 
                [as].*, [app].[Name] AS [AppName], [a].[Name] AS [ActivityName]
			FROM 
                [dbo].[ActivitySubscription] [as] 
			    INNER JOIN [dbo].[Activity] [a] ON [as].[ActivityId]=[a].[Id]
			    INNER JOIN [dbo].[Application] [app] ON [as].[ApplicationId]=[app].[Id]
			WHERE 
                [as].[UserId]=@userId AND 
                [as].[OrganizationId]=@orgId {andWhere}")
		{
		}

		public int OrgId { get; set; }
		public int UserId { get; set; }

        [Where("[InApp]=@inApp")]
        public bool? InApp { get; set; }
	}
}