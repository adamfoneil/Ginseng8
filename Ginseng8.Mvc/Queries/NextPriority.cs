using System.Collections.Generic;
using System.Data;
using Dapper.QX;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
	public class NextPriorityResult
	{
		public int NextValue { get; set; }
	}

	public class NextPriority : Query<NextPriorityResult>, ITestableQuery
	{
		public NextPriority() : base(
			@"SELECT
				COALESCE(MAX([wip].[Value]), 1) AS [NextValue]
			FROM
				[dbo].[WorkItemPriority] [wip]
				INNER JOIN [dbo].[WorkItem] [wi] ON [wip].[WorkItemId]=[wi].[Id]
			WHERE
				[wip].[UserId]=0 AND
				[wip].[MilestoneId]=0 AND
				[wi].[OrganizationId]=@orgId AND
				[wi].[TeamId]=@teamId AND
				[wi].[CloseReasonId] IS NULL")
		{
		}

		public int OrgId { get; set; }
		public int TeamId { get; set; }

		public IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new NextPriority() { OrgId = 0, TeamId = 0 };
		}

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}
	}
}