CREATE FUNCTION [dbo].[FnDateRange](
	@start date, @end date
) RETURNS @results TABLE (
	[Date] date NOT NULL,
	[DayOfWeek] int NOT NULL,
	[Flag] int NOT NULL,
	[WeekNumber] int NOT NULL
) AS
BEGIN
	DECLARE @date date
	SET @date = @start
	WHILE @date <= @end
	BEGIN
		INSERT INTO @results (
			[Date], [DayOfWeek], [Flag], [WeekNumber]
		) SELECT
			@date, DATEPART(dw, @date), [wd].[Flag], DATEPART(ww, @date)
		FROM
			[app].[WorkDay] [wd]
		WHERE
			[Value]=DATEPART(dw, @date)

		SET @date = DATEADD(d, 1, @date)
	END
	RETURN
END

