ALTER FUNCTION [dbo].[FnColorGradientPositions](
	@orgId int
) RETURNS @results TABLE (
	[Id] int NOT NULL,
	[MinHours] int NOT NULL,
	[MaxHours] int NULL,
	[ColorGradientPosition] float NOT NULL
) AS
BEGIN
	WITH [source] AS (
		SELECT 
			[wis].*,		
			ROW_NUMBER() OVER (ORDER BY [EstimateHours]) - 1 AS [RowNumber]
		FROM 
			[dbo].[WorkItemSize] [wis]
		WHERE 
			[OrganizationId]=@orgId
	), [maxRow] AS (
		SELECT MAX([RowNumber]) AS [RowCount] FROM [source]
	) INSERT INTO @results (
		[Id], [MinHours], [MaxHours], [ColorGradientPosition]
	) SELECT
		[Id],
		[EstimateHours],
		COALESCE((SELECT MIN([EstimateHours]) FROM [dbo].[WorkItemSize] WHERE [OrganizationId]=@orgId AND [EstimateHours]>[source].[EstimateHours]), 1000),
		CONVERT(float, [RowNumber]) / CONVERT(float, [RowCount]) AS [ColorGradientPosition]
	FROM
		[source],
		[maxRow]	
	ORDER BY
		[EstimateHours]

	RETURN
END
