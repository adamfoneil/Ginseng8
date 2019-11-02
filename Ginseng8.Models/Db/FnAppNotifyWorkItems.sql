CREATE FUNCTION [dbo].[FnAppNotifyWorkItems](
	@orgId int,
	@sendTo varchar(50)
) RETURNS @results TABLE (
	[WorkItemId] int NOT NULL
) AS
BEGIN	
	INSERT INTO @results (
		[WorkItemId]
	) SELECT
		[WorkItemId]
	FROM
		[dbo].[FnAppNotifications](@orgId, @sendTo)
	GROUP BY
		[WorkItemId]
	RETURN
END
