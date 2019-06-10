CREATE FUNCTION [dbo].[FnUserWorkDays](
	@orgId int	
) RETURNS @results TABLE (
	[UserId] int NOT NULL,
	[Value] int NOT NULL,
	[Abbreviation] nvarchar(3) NOT NULL,
	[Hours] decimal(4,2) NOT NULL
) AS
BEGIN
	INSERT INTO @results (
		[UserId], [Value], [Abbreviation], [Hours]
	) SELECT
		[ou].[UserId], [wd].[Value], [wd].[Abbreviation], [ou].[DailyWorkHours]
	FROM
		[app].[WorkDay] [wd]
		INNER JOIN [dbo].[OrganizationUser] [ou] ON (([wd].[Flag] & [ou].[WorkDays]) = [wd].[Flag])
	WHERE
		[ou].[OrganizationId] = @orgId

	RETURN
END
