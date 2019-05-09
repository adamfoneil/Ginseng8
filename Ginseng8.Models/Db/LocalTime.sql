CREATE FUNCTION [dbo].[LocalTime](
	@userName nvarchar(50),	
	@utcDateTime datetime
) RETURNS datetime AS
BEGIN
	DECLARE @result datetime
	SET @result = @utcDateTime

	DECLARE @offset int, @dst bit
	SELECT @offset = [TimeZoneOffset], @dst = [AdjustForDaylightSaving] FROM [dbo].[AspNetUsers] WHERE [UserName]=@userName	

	IF @offset < 24
	BEGIN
		SET @result = DATEADD(hh, @offset + @dst, @result)
	END
	ELSE
	BEGIN
		SET @result = DATEADD(n, @offset, @result)
	END

	RETURN @result
END