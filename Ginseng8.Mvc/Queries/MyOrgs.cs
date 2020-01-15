using Ginseng.Models;
using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
	public class MyOrgs : Query<Organization>
	{
		public MyOrgs() : base(
			@"SELECT
				[org].*
			FROM
				[dbo].[Organization] [org]
			WHERE
				[org].[OwnerUserId]=@userId
			ORDER BY
				[Name]")
		{
		}

		public int UserId { get; set; }
	}
}