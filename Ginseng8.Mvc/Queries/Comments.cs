using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class Comments : Query<Comment>
	{
		public Comments() : base(
			@"SELECT
				[c].*,
				COALESCE([ou].[DisplayName], [u].[UserName]) AS [DisplayName]
			FROM
				[dbo].[Comment] [c]				
				INNER JOIN [dbo].[AspNetUsers] [u] ON [c].[CreatedBy]=[u].[UserName]
				LEFT JOIN [dbo].[OrganizationUser] [ou] ON
					[c].[OrganizationId]=[ou].[OrganizationId] AND
					[ou].[UserId]=[u].[UserId]
			WHERE	
				[c].[OrganizationId]=@orgId AND
				[c].[ObjectType]=@objectType AND
				[c].[ObjectId] IN @objectIds
			ORDER BY
				[c].[DateCreated] DESC")
		{
		}
		
		public int OrgId { get; set; }
		public ObjectType ObjectType { get; set; }
		public int[] ObjectIds { get; set; }
	}
}