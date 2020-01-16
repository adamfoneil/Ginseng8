using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
	public class LabelsInUse : Query<Label>, ITestableQuery
	{
		public LabelsInUse() : base(
			@"SELECT
				[wil].[WorkItemId],
				[l].*
			FROM
				[dbo].[WorkItemLabel] [wil]
				INNER JOIN [dbo].[Label] [l] ON [wil].[LabelId]=[l].[Id]
				INNER JOIN [dbo].[WorkItem] [wi] ON [wil].[WorkItemId]=[wi].[Id]
			WHERE
				[wil].[WorkItemId] IN @workItemIds AND
				[wi].[OrganizationId]=@orgId
			ORDER BY
				[wil].[WorkItemId],
				[l].[Name]")
		{
		}

		public int OrgId { get; set; }
		public int[] WorkItemIds { get; set; }

		public IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new LabelsInUse() { OrgId = 1, WorkItemIds = new int[] { 1, 2, 3 } };
		}

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}
	}
}