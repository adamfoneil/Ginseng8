CREATE FUNCTION [dbo].[FnPriorityTierRanges](
	@orgId int
) RETURNS @results TABLE (
	[Name] nvarchar(50) NOT NULL,
	[MaxProjects] int NOT NULL,
	[Rank] int NOT NULL,
	[MinPriority] int NOT NULL,
	[MaxPriority] int NOT NULL
) AS
BEGIN
	INSERT INTO @results (
		[Name], [MaxProjects], [Rank], [MinPriority], [MaxPriority]
	) SELECT 
		[current].[Name],
		[current].[MaxProjects],
		[current].[Rank],		
		ISNULL([prior].[LoBound], 0) + 1 AS [MinPriority],
		[current].[LoBound] AS [MaxPriority]
	FROM 
		[dbo].[FnPriorityTiers](@orgId) [current]
		LEFT JOIN [dbo].[FnPriorityTiers](@orgId) [prior] ON [current].[Rank]-1 = [prior].[Rank]
	RETURN
END