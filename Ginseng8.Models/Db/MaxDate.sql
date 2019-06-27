CREATE FUNCTION [dbo].[MaxDate](
	@date1 date,
	@date2 date
) RETURNS datetime AS
BEGIN
	IF @date1 >= @date2 RETURN @date1
	RETURN @date2
END
