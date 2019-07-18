CREATE PROC [dbo].[UpdateWorkItemUserPriorities]
	@userName nvarchar(50),
	@localTime datetime,
	@orgId int,
	@milestoneId int, -- this is the work day number, but we're using the same params as dbo.UpdateWorkItemPriorities, so we're stuck with this name
	@userId int,
	@priorities [WorkItemPriority] readonly
AS
SET NOCOUNT ON

INSERT INTO [dbo].[WorkItemUserPriority] (
	[WorkItemId],  [UserId], [WorkDay], [Value], [CreatedBy], [DateCreated]
) SELECT
	[wi].[Id], @userId, @milestoneId, [p].[Index]+1, @userName, @localTime
FROM
	[dbo].[WorkItem] [wi]
	INNER JOIN @priorities [p] ON [wi].[Number]=[p].[Number]
WHERE
	[wi].[OrganizationId]=@orgId AND
	NOT EXISTS(SELECT 1 FROM [dbo].[WorkItemUserPriority] WHERE [WorkItemId]=[wi].[Id])

UPDATE [wip] SET
	[Value]=[p].[Index]+1,
	[UserId]=@userId,
	[WorkDay]=@milestoneId,
	[ModifiedBy]=@userName,
	[DateModified]=@localTime
FROM
	[dbo].[WorkItemUserPriority] [wip]
	INNER JOIN [dbo].[WorkItem] [wi] ON [wi].[Id]=[wip].[WorkItemId]
	INNER JOIN @priorities [p] ON [wi].[Number]=[p].[Number]
WHERE
	[wi].[OrganizationId]=@orgId