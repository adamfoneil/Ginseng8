ALTER PROC [dbo].[CancelInvoice]
	@userName nvarchar(50),
	@orgId int,
	@number int
AS
SET NOCOUNT ON

IF NOT EXISTS(
	SELECT 1 FROM [dbo].[OrganizationUser] [ou] INNER JOIN [dbo].[AspNetUsers] [u] ON [ou].[UserId]=[u].[UserId]
	WHERE [u].[UserName]=@userName AND [ou].[OrganizationId]=@orgId AND [ou].[IsEnabled]=1)
BEGIN
	RAISERROR('User does not have permission to cancel invoices.', 16, 1)
	RETURN
END

IF EXISTS(SELECT 1 FROM [dbo].[Invoice] WHERE [StatusId]=2 AND [Number]=@number AND [OrganizationId]=@orgId)
BEGIN
	RAISERROR('Can''t cancel a paid invoice.', 16, 1)
	RETURN
END

BEGIN TRY

	BEGIN TRAN

	INSERT INTO [dbo].[PendingWorkLog] (
		[OrganizationId], [ProjectId], [WorkItemId], [ApplicationId], [UserId], [Date], [Hours], [HtmlBody], [TextBody], [CreatedBy], [DateCreated]
	) SELECT
		[iwl].[OrganizationId], [ProjectId], [WorkItemId], [iwl].[ApplicationId], [UserId], [Date], [Hours], [HtmlBody], [TextBody], [iwl].[CreatedBy], [iwl].[DateCreated]
	FROM
		[dbo].[InvoiceWorkLog] [iwl]
		INNER JOIN [dbo].[Invoice] [inv] ON [iwl].[InvoiceId]=[inv].[Id]
	WHERE
		[inv].[OrganizationId]=@orgId AND
		[inv].[Number]=@number

	DELETE [iwl]
	FROM 
		[dbo].[InvoiceWorkLog] [iwl]
		INNER JOIN [dbo].[Invoice] [inv] ON [iwl].[InvoiceId]=[inv].[Id]
	WHERE
		[inv].[OrganizationId]=@orgId AND
		[inv].[Number]=@number
	
	UPDATE [inv] SET
		[StatusId]=4, --canceled
		[StatusDate]=dbo.LocalTimeNow(@userName),
		[ModifiedBy]=@userName,
		[DateModified]=dbo.LocalTimeNow(@userName)
	FROM
		[dbo].[Invoice] [inv]
	WHERE
		[inv].[OrganizationId]=@orgId AND
		[inv].[Number]=@number

END TRY
BEGIN CATCH
	IF @@TRANCOUNT > 0 ROLLBACK TRAN
	DECLARE @message nvarchar(max)
	SET @message = ERROR_MESSAGE()
	RAISERROR(@message, 16, 1)
END CATCH

IF @@TRANCOUNT > 0 COMMIT TRAN