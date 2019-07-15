CREATE PROC [dbo].[UpdateUserActivityOrder]
	@userName nvarchar(50),
	@localTime datetime,
	@userId int,
	@orgId int,
	@priorities [WorkItemPriority] readonly
AS
SET NOCOUNT ON

DELETE [uao]
FROM [dbo].[UserActivityOrder] [uao]
INNER JOIN [dbo].[Activity] [a] ON [uao].[ActivityId]=[a].[Id]
WHERE [uao].[UserId]=@userId AND [a].[OrganizationId]=@orgId

INSERT INTO [dbo].[UserActivityOrder] (
	[ActivityId], [UserId], [Value], [CreatedBy], [DateCreated]
) SELECT
	[p].[Number], @userId, [p].[Index] + 1, @userName, @localTime
FROM
	@priorities [p]
