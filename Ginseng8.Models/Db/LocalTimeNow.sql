CREATE FUNCTION [dbo].[LocalTimeNow](
	@userName nvarchar(50)
) RETURNS datetime AS
BEGIN
	DECLARE @result datetime
	SET @result = dbo.LocalTime(@userName, getutcdate())
	RETURN @result
END