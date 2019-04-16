CREATE FUNCTION [dbo].[FnPriorityTiers](
	@orgId int
) RETURNS @results TABLE (
	[Name] nvarchar(50) NOT NULL, 
	[MaxProjects] int NOT NULL, 
	[Rank] int NOT NULL,
	[LoBound] int NOT NULL
) AS
BEGIN
	INSERT INTO @results (
		[Name], [MaxProjects], [Rank], [LoBound]
	) SELECT 
		[pt].[Name], 
		[pt].[MaxProjects],
		[pt].[Rank],
		SUM([pt].[MaxProjects]) OVER (ORDER BY [Rank]) AS [LoBound]
	FROM 
		[dbo].[PriorityTier] [pt]
	WHERE 
		[pt].[OrganizationId]=@orgId
	RETURN
END
