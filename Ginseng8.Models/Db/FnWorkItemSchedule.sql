ALTER FUNCTION [dbo].[FnWorkItemSchedule](
	@orgId int,
	@userId int
) RETURNS @results TABLE (
	[Number] int NOT NULL,
	[Date] date NOT NULL
) AS
BEGIN

	DECLARE @workHours TABLE (
		[Date] date NOT NULL,
		[DayNumber] int NOT NULL,
		[Hour] int NOT NULL
	);

	INSERT INTO @workHours (
		[Date], [DayNumber], [Hour]
	) SELECT
		[Date], [DayNumber], ROW_NUMBER() OVER (ORDER BY [DayNumber]) AS [Hour]
	FROM
		[dbo].[FnWorkHourRanges](@orgId, @userId) [whr]
		CROSS APPLY [dbo].[FnIntRange]([whr].[StartHours], [whr].[EndHours])

	DECLARE @openItems TABLE (
		[Number] int NOT NULL,
		[EstimateHours] int NOT NULL,
		[Hour] int
	);

	INSERT INTO @openItems (
		[Number], [EstimateHours], [Hour]
	) SELECT
		[Number], [EstimateHours], ROW_NUMBER() OVER (ORDER BY [Priority]) AS [Hour]
	FROM
		[dbo].[FnOpenItemEstimateRanges](@orgId, @userId) [oie]
		CROSS APPLY [dbo].[FnIntRange]([oie].[StartHours], [oie].[EndHours])

	INSERT INTO @results (
		[Number], [Date]
	) SELECT
		[oi].[Number], [wh].[Date]
	FROM
		@workHours [wh] INNER JOIN @openItems [oi] ON [wh].[Hour]=[oi].[Hour]
	GROUP BY
		[oi].[Number], [wh].[Date]

	RETURN
END
GO

SELECT * FROM [dbo].[FnWorkItemSchedule](1, 10) ORDER BY [Date]