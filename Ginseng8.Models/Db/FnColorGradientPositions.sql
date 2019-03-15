CREATE FUNCTION [dbo].[FnColorGradientPositions](
	@orgId int
) RETURNS @results TABLE (
	[Id] int NOT NULL,
	[EstimateHours] int NOT NULL,
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
		[Id], [EstimateHours], [ColorGradientPosition]
	) SELECT
		[Id],
		[EstimateHours],
		CONVERT(float, [RowNumber]) / CONVERT(float, [RowCount]) AS [ColorGradientPosition]
	FROM
		[source],
		[maxRow]	
	ORDER BY
		[EstimateHours]

	RETURN
END
