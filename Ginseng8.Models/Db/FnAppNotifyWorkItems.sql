CREATE FUNCTION [dbo].[FnAppNotifyWorkItems](
	@orgId int,
	@sendTo varchar(50)
) RETURNS @results TABLE (
	[Id] int NOT NULL,
	[HtmlBody] nvarchar(max) NOT NULL
) AS
BEGIN
	WITH [source] AS (
		SELECT TOP 100 PERCENT
			[el].[WorkItemId],
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
		[Id], [HtmlBody]
	) SELECT TOP (10)
		[WorkItemId], [HtmlBody]
	FROM
		[source]
	GROUP BY
		[WorkItemId], [HtmlBody]
	RETURN
END
