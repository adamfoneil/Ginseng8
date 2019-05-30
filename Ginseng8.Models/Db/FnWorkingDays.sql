CREATE FUNCTION [dbo].[FnWorkingDays](
	@orgId int,
	@start date, 
	@end date
) RETURNS @results TABLE (
	[UserId] int NOT NULL,
	[Date] date NOT NULL,
	[Hours] int NOT NULL,
	[DayOfWeek] int NOT NULL,
	[Flag] int NOT NULL,
	[WeekNumber] int NOT NULL,
	[DayNumber] int NOT NULL
) AS
BEGIN
	INSERT INTO @results (
		[UserId], [Date], [Hours], [DayOfWeek], [Flag], [WeekNumber], [DayNumber]
	) SELECT
		[ou].[UserId], [dr].[Date], [ou].[DailyWorkHours] - ISNULL([vh].[Hours], 0), [dr].[DayOfWeek], [dr].[Flag], [dr].[WeekNumber], ROW_NUMBER() OVER (PARTITION BY [ou].[UserId] ORDER BY [dr].[Date])
	FROM
		[dbo].[FnDateRange](@start, @end) [dr]
		INNER JOIN [dbo].[OrganizationUser] [ou] ON (([dr].[Flag] & [ou].[WorkDays]) = [dr].[Flag])
		LEFT JOIN [dbo].[VacationHours] [vh] ON 
			[vh].[OrganizationId]=[ou].[OrganizationId] AND
			[vh].[UserId]=[ou].[UserId] AND
			[vh].[Date]=[dr].[Date]		
	WHERE
		[ou].[OrganizationId]=@orgId 

	DELETE @results WHERE [Hours]<=0
	RETURN
END