using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
	/// <summary>
	/// Creates a join request org user record for the given orgName and userId
	/// </summary>
	public class CreateOrgUserJoinRequest : Query<int>
	{
		public CreateOrgUserJoinRequest() : base(
			@"INSERT INTO [dbo].[OrganizationUser] (
				[OrganizationId], [UserId], [IsEnabled], [IsRequest], [Responsibilities], [WorkDays], [DailyWorkHours], [CreatedBy], [DateCreated]
			) SELECT
				[org].[Id], @userId, 0, 1, 0, 0, 0, [u].[UserName], GETUTCDATE()
			FROM
				[dbo].[Organization] [org],
				[dbo].[AspNetUsers] [u]
			WHERE
				NOT EXISTS(SELECT 1 FROM [dbo].[OrganizationUser] WHERE [OrganizationId]=[org].[Id] AND [UserId]=@userId) AND
				[org].[Name]=@orgName AND
				[u].[UserId]=@userId; SELECT SCOPE_IDENTITY()")
		{
		}

		public string OrgName { get; set; }
		public int UserId { get; set; }
	}
}