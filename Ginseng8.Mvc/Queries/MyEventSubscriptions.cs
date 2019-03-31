using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class MyEventSubscriptions : Query<EventSubscription>
	{
		public MyEventSubscriptions() : base(
			@"WITH [source] AS (
				SELECT 
					[es].*
				FROM 
					[dbo].[EventSubscription] [es]
				WHERE
					[UserId]=@userId AND
					[OrganizationId]=@orgId AND
					[ApplicationId]=@appId
			) SELECT
				[source].*
			FROM
				[source]
			UNION
			SELECT 
				0 AS [Id],
				[ev].[Id] AS [EventId],
				@orgId AS [OrganizationId],
				@appId AS [AppliationId],
				@userId AS [UserId],
				1 AS [Visible],
				0 AS [SendEmail],
				0 AS [SendText],
				0 AS [InApp],
				NULL AS [CreatedBy],
				NULL AS [DateCreated],
				NULL AS [ModifiedBy],
				NULL AS [DateModified]
			FROM
				[app].[Event] [ev]
			WHERE
				NOT EXISTS(SELECT 1 FROM [source] WHERE [EventId]=[ev].[Id])")
		{
		}

		public int UserId { get; set; }
		public int OrgId { get; set; }
		public int AppId { get; set; }
	}
}