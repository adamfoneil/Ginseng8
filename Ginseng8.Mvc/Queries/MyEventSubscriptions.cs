using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class MyEventSubscriptions : Query<EventSubscription>
	{
		public MyEventSubscriptions() : base(
            @"SELECT 
				[es].*, 
                [app].[Name] AS [ApplicationName],
                [ev].[Name] AS [EventName]
			FROM 
				[dbo].[EventSubscription] [es]
                INNER JOIN [dbo].[Application] [app] ON [es].[ApplicationId]=[app].[Id]
                INNER JOIN [app].[Event] [ev] ON [es].[EventId]=[ev].[Id]
			WHERE
				[es].[UserId]=@userId AND
				[es].[OrganizationId]=@orgId {andWhere}")
		{
		}

		public int UserId { get; set; }
		public int OrgId { get; set; }

        [Where("[es].[ApplicationId]=@appId")]
		public int? AppId { get; set; }

        [Where("[es].[InApp]=@inApp")]
        public bool? InApp { get; set; }
	}
}