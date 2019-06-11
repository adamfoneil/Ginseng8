CREATE FUNCTION [dbo].[FnWorkHourRanges](
	@orgId int,
	@userId int
) RETURNS @results TABLE (
	[Date] date NOT NULL,
	[DayNumber] int NOT NULL,
	[StartHours] int NOT NULL,
	[EndHours] int NOT NULL,
	[WeekNumber] int NOT NULL
) AS
BEGIN
	DECLARE @userName nvarchar(50)
	SELECT @userName = [UserName] FROM [dbo].[AspNetUsers] WHERE [UserId]=@userId

	DECLARE @startDate date, @endDate date
	SET @startDate = dbo.LocalTimeNow(@userName)
	SET @endDate = DATEADD(d, 30, @startDate);

	DECLARE @workHours TABLE (
		[Date] date NOT NULL,
		[DayNumber] int NOT NULL,
		[Hours] int NOT NULL,
		[RunningHours] int NOT NULL
	);

	WITH [workDays] AS (
		SELECT
			[wd].[Date], [wd].[Hours]
		FROM
			[dbo].[FnWorkingDays](@orgId, @startDate, @endDate) [wd]
		WHERE
			[UserId]=@userId
	) INSERT INTO @workHours (
		[Date], [Hours], [RunningHours], [DayNumber]
	) SELECT 
		[wd].[Date], [wd].[Hours],
		SUM([Hours]) OVER (ORDER BY [Date]) AS [RunningHours],
		ROW_NUMBER() OVER (ORDER BY [Date]) AS [DayNumber]
	FROM 
		[workDays] [wd];

	INSERT INTO @results (
		[Date], [DayNumber], [StartHours], [EndHours], [WeekNumber]
	) SELECT
		[current].[Date], [current].[DayNumber], COALESCE([prev].[RunningHours] + 1, 1), [current].[RunningHours],
		DATEPART(ww, [current].[Date])
	FROM
		@workHours [current]
		LEFT JOIN @workHours [prev] ON [current].[DayNumber]-1 = [prev].[DayNumber]

	RETURN
END

