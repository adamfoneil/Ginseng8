CREATE PROC [dbo].[UpdateModelPropertyOrder]
	@userName nvarchar(50),
	@localTime datetime,
	@orgId int,
	@propertyOrder [WorkItemPriority] readonly
AS
SET NOCOUNT ON

UPDATE [mp] SET
	[Position]=[p].[Index]+1,
	[ModifiedBy]=@userName,
	[DateModified]=@localTime
FROM
	[dbo].[ModelProperty] [mp]
	INNER JOIN @propertyOrder [p] ON [mp].[Id]=[p].[Number]
	INNER JOIN [dbo].[ModelClass] [mc] ON [mp].[ModelClassId]=[mc].[Id]
	INNER JOIN [dbo].[DataModel] [dm] ON [mc].[DataModelId]=[dm].[Id]
	INNER JOIN [dbo].[Application] [app] ON [dm].[ApplicationId]=[app].[Id]
WHERE
	[app].[OrganizationId]=@orgId