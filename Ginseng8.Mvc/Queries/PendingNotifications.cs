using System.Collections.Generic;
using System.Data;
using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
	public class PendingNotifications : Query<Notification>, ITestableQuery
	{
		public PendingNotifications(int count) : base(
			$@"SELECT TOP ({count}) [n].*, [el].[IconClass], [el].[IconColor]
			FROM [dbo].[Notification] [n]
			INNER JOIN [dbo].[EventLog] [el] ON [n].[EventLogId]=[el].[Id]
			WHERE [DateDelivered] IS NULL {{andWhere}} ORDER BY [n].[DateCreated] ASC")
		{
		}

		[Where("[Method]=@method")]
		public DeliveryMethod? Method { get; set; }

		public IEnumerable<dynamic> TestExecute(IDbConnection connection)
		{
			return TestExecuteHelper(connection);
		}

		public static IEnumerable<ITestableQuery> GetTestCases()
		{
			yield return new PendingNotifications(10);
			yield return new PendingNotifications(10) { Method = DeliveryMethod.Email };
		}
	}
}