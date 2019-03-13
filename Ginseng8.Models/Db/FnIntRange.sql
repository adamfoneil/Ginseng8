CREATE FUNCTION [dbo].[FnIntRange](
	@start int,
	@end int
) RETURNS @results TABLE (
	[Value] int NOT NULL
) AS
BEGIN
	DECLARE @value int
	SET @value = @start
	WHILE @value <= @end
	BEGIN
		INSERT INTO @results ([Value]) VALUES (@value)
		SET @value = @value + 1
	END
	RETURN
END