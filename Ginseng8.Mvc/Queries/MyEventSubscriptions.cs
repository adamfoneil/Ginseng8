using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class MyEventSubscriptions : Query<EventSubscription>
	{
		public MyEventSubscriptions() : base(
			@"SELECT 
				[es].*
			FROM 
				[dbo].[EventSubscription] [es]
			WHERE
				[UserId]=@userId AND
				[OrganizationId]=@orgId AND
				[ApplicationId]=@appId")
		{
		}

		public int UserId { get; set; }
		public int OrgId { get; set; }
		public int AppId { get; set; }
	}
}