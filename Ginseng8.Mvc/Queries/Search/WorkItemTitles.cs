using Ginseng.Models;
using Dapper.QX;

namespace Ginseng.Mvc.Queries.Search
{
	public class WorkItemTitles : Query<WorkItem>
	{
		public WorkItemTitles() : base(
			@"SELECT TOP (10)
				[wi].[Number],
				[wi].[Title],
				[wi].[DateCreated],
			FROM 
				[dbo].[WorkItem] [wi]
			WHERE
				[OrganizationId]=@orgId AND 
				[Title] LIKE '%'+@title+'%'
			ORDER BY
				[Number]")
		{
		}

		public int OrgId { get; set; }
	}
}