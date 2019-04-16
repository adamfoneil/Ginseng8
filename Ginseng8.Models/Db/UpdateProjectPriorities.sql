CREATE PROC [dbo].[UpdateProjectPriorities]
	@userName nvarchar(50),
	@localTime datetime,
	@orgId int,
	@priorities [WorkItemPriority] readonly
AS
SET NOCOUNT ON

UPDATE [prj] SET
	[Priority]=[p].[Index]+1,
	[ModifiedBy]=@userName,
	[DateModified]=@localTime
FROM
	[dbo].[Project] [prj]	
	INNER JOIN [dbo].[Application] [app] ON [prj].[ApplicationId]=[app].[Id]
	INNER JOIN @priorities [p] ON [prj].[Id]=[p].[Number]
WHERE
	[app].[OrganizationId]=@orgId