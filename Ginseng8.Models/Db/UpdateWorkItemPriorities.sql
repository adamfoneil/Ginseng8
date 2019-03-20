ALTER PROC [dbo].[UpdateWorkItemPriorities]
	@userName nvarchar(50),
	@localTime datetime,
	@orgId int,
	@milestoneId int,
	@userId int,
	@priorities [WorkItemPriority] readonly
AS
SET NOCOUNT ON

INSERT INTO [dbo].[WorkItemPriority] (
	[WorkItemId], [MilestoneId], [UserId], [Value], [CreatedBy], [DateCreated]
) SELECT
	[wi].[Id], @milestoneId, @userId, [p].[Index]+1, @userName, @localTime
FROM
	[dbo].[WorkItem] [wi]
	INNER JOIN @priorities [p] ON [wi].[Number]=[p].[Number]
WHERE
	[wi].[OrganizationId]=@orgId AND
	NOT EXISTS(SELECT 1 FROM [dbo].[WorkItemPriority] WHERE [WorkItemId]=[wi].[Id])

UPDATE [wip] SET
	[Value]=[p].[Index]+1,
	[UserId]=@userId,
	[MilestoneId]=@milestoneId,
	[ModifiedBy]=@userName,
	[DateModified]=@localTime
FROM
	[dbo].[WorkItemPriority] [wip]
	INNER JOIN [dbo].[WorkItem] [wi] ON [wi].[Id]=[wip].[WorkItemId]
	INNER JOIN @priorities [p] ON [wi].[Number]=[p].[Number]
WHERE
	[wi].[OrganizationId]=@orgId