ALTER PROC [dbo].[PostInvoice]
	@userName nvarchar(50),
	@orgId int,
	@appId int
AS
SET NOCOUNT ON

IF NOT EXISTS(
	SELECT 1 FROM [dbo].[OrganizationUser] [ou] INNER JOIN [dbo].[AspNetUsers] [u] ON [ou].[UserId]=[u].[UserId]
	WHERE [u].[UserName]=@userName AND [ou].[OrganizationId]=@orgId AND [ou].[IsEnabled]=1)
BEGIN
	RAISERROR('User does not have permission to post invoices.', 16, 1)
	RETURN
END

IF NOT EXISTS(SELECT 1 FROM [dbo].[Application] WHERE [OrganizationId]=@orgId AND [Id]=@appId)
BEGIN
	RAISERROR('Application and Org Id parameters are invalid.', 16, 1)
	RETURN
END

DECLARE @invNumber int
SELECT @invNumber = [NextInvoiceNumber] FROM [dbo].[Organization] WHERE [Id]=@orgId

DECLARE @localTime datetime
SET @localTIme = dbo.LocalTimeNow(@userName)

BEGIN TRY

	BEGIN TRAN

	INSERT INTO [dbo].[Invoice] (
		[OrganizationId], [Number], [ApplicationId], [Amount], [StatusId], [CreatedBy], [DateCreated]
	) SELECT
		@orgId, @invNumber, @appId, SUM([wl].[Hours]*[ou].[InvoiceRate]), 1, @userName, @localTime
	FROM
		[dbo].[PendingWorkLog] [wl]		
		INNER JOIN [dbo].[OrganizationUser] [ou] ON [ou].[UserId]=[wl].[UserId] AND [ou].[OrganizationId]=[wl].[OrganizationId]
	WHERE
		[wl].[ApplicationId]=@appId AND
		[wl].[OrganizationId]=@orgId

	DECLARE @invoiceId int
	SELECT @invoiceId = SCOPE_IDENTITY()

	INSERT INTO [dbo].[InvoiceWorkLog] (
		[InvoiceId], [OrganizationId], [ProjectId], [WorkItemId], [ApplicationId], [UserId], [Date], [Hours], [Rate], 
		[TextBody], [HtmlBody], [SourceType], [SourceId], [PendingId], [CreatedBy], [DateCreated]
	) SELECT
		@invoiceId, [wl].[OrganizationId], [wl].[ProjectId], [wl].[WorkItemId], [wl].[ApplicationId], [wl].[UserId], [wl].[Date], [wl].[Hours], [ou].[InvoiceRate], 
		[wl].[TextBody], [wl].[HtmlBody], [wl].[SourceType], [wl].[SourceId], [wl].[Id], @userName, @localTime
	FROM
		[dbo].[PendingWorkLog] [wl]		
		INNER JOIN [dbo].[OrganizationUser] [ou] ON [ou].[UserId]=[wl].[UserId] AND [ou].[OrganizationId]=[wl].[OrganizationId]
	WHERE
		[wl].[ApplicationId]=@appId AND
		[wl].[OrganizationId]=@orgId

	DELETE [wl]
	FROM [dbo].[PendingWorkLog] [wl] INNER JOIN [dbo].[InvoiceWorkLog] [iwl] ON [wl].[Id]=[iwl].[PendingId]

	UPDATE [org] SET
		[NextInvoiceNumber]=@invNumber + 1
	FROM
		[dbo].[Organization] [org]
	WHERE
		[Id]=@orgId

END TRY
BEGIN CATCH	
	IF @@TRANCOUNT > 0 ROLLBACK TRAN
	DECLARE @message nvarchar(max)
	SET @message = ERROR_MESSAGE()
	RAISERROR(@message, 16, 1)
END CATCH

IF @@TRANCOUNT > 0 COMMIT TRAN
GO