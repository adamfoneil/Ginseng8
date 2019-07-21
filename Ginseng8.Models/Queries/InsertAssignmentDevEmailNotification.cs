using Postulate.Base;

namespace Ginseng.Models.Queries
{
    public class InsertAssignmentDevEmailNotification : Query<int>
    {
        public InsertAssignmentDevEmailNotification() : base(
            @"INSERT INTO [dbo].[Notification] (
				[EventLogId], [DateCreated], [Method], [SendTo], [Content], [SourceId], [SourceTable]
			) SELECT
				@id, getutcdate(), 1, [u].[Email], [el].[HtmlBody], [ou].[Id], 'OrganizationUser'
			FROM
				[dbo].[EventLog] [el]
				INNER JOIN [dbo].[WorkItem] [wi] ON [el].[WorkItemId]=[wi].[Id]
				INNER JOIN [dbo].[AspNetUsers] [u] ON [wi].[DeveloperUserId]=[u].[UserId]
                INNER JOIN [dbo].[OrganizationUser] [ou] ON
                    [ou].[OrganizationId]=[wi].[OrganizationId] AND
                    [ou].[UserId]=[wi].[DeveloperUserId]
			WHERE
				[el].[Id]=@id AND
				[ou].[SendEmail]=1 AND
				([u].[EmailConfirmed]=1 OR [u].[PasswordHash] IS NULL)")
        {
        }

        public int Id { get; set; }
    }
}