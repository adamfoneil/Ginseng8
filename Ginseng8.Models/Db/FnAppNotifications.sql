CREATE FUNCTION [dbo].[FnAppNotifications](
	@orgId int,
	@sendTo varchar(50)
) RETURNS @results TABLE (
	[WorkItemId] int NOT NULL,
	[NotificationId] int NOT NULL,
	[HtmlBody] nvarchar(max) NOT NULL
) AS
BEGIN
	WITH [source] AS (
		SELECT TOP 100 PERCENT
			[el].[WorkItemId],
			[n].[Id] AS [NotificationId],
			[el].[DateCreated],
			[el].[HtmlBody]
		FROM
			[dbo].[EventLog] [el]
			INNER JOIN [dbo].[Notification] [n] ON [el].[Id]=[n].[EventLogId]
		WHERE
			[n].[Method]=3 AND
			[n].[DateDelivered] IS NULL AND
			[n].[SendTo]=@sendTo AND
			[el].[OrganizationId]=@orgId
		ORDER BY
			[el].[DateCreated] ASC
	) INSERT INTO @results (
		[WorkItemId], [NotificationId], [HtmlBody]
	) SELECT TOP (10)
		[WorkItemId], [NotificationId], [HtmlBody]
	FROM
		[source]
	GROUP BY
		[WorkItemId], [NotificationId], [HtmlBody]
	RETURN
END
