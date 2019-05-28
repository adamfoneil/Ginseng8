ALTER FUNCTION [dbo].[FnOpenItemEstimateRanges](
	@orgId int,
	@userId int
) RETURNS @results TABLE (
	[Number] int NOT NULL,
	[EstimateHours] int NOT NULL,
	[StartHours] int NOT NULL,
	[EndHours] int NOT NULL,
	[Priority] int NOT NULL
) AS
BEGIN

	DECLARE @openItems TABLE (
		[Number] int NOT NULL,
		[EstimateHours] int NOT NULL,
		[RunningHours] int NOT NULL,
		[RowNumber] int NOT NULL
	);

	WITH [estimates] AS (
		SELECT
			[ms].[Date] AS [MilestoneDate],		
			[wi].[Number],				
			COALESCE([wid].[EstimateHours], [sz].[EstimateHours], 0) AS [Hours],
			[wip].[Value] AS [Priority]
		FROM
			[dbo].[WorkItem] [wi]
			INNER JOIN [dbo].[Milestone] [ms] ON [wi].[MilestoneId]=[ms].[Id]
			LEFT JOIN [dbo].[WorkItemPriority] [wip] ON [wi].[Id]=[wip].[WorkItemId]
			LEFT JOIN [dbo].[WorkItemSize] [sz] ON [wi].[SizeId]=[sz].[Id]
			LEFT JOIN [dbo].[WorkItemDevelopment] [wid] ON [wi].[Id]=[wid].[WorkItemId]
		WHERE
			[wi].[OrganizationId]=@orgId AND        
			[wi].[DeveloperUserId]=@userId AND
			[wi].[CloseReasonId] IS NULL		
	) INSERT INTO @openItems (
		[Number], [EstimateHours], [RunningHours], [RowNumber]
	) SELECT 
		[e].[Number], [e].[Hours], SUM([Hours]) OVER (ORDER BY [MilestoneDate], [Priority]),
		ROW_NUMBER() OVER (ORDER BY [MilestoneDate], [Priority])
	FROM 
		[estimates] [e]
	ORDER BY 
		[MilestoneDate], [Priority];

	INSERT INTO @results (
		[Number], [EstimateHours], [StartHours], [EndHours], [Priority]
	) SELECT
		[current].[Number], [current].[EstimateHours],
		ISNULL([prev].[RunningHours] + 1, 1),
		[current].[RunningHours],
		[current].[RowNumber]
	FROM
		@openItems [current]
		LEFT JOIN @openItems [prev] ON [current].[RowNumber]-1 = [prev].[RowNumber]

	RETURN
END
