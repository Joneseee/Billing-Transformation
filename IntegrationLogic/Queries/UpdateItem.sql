BEGIN TRY

	DECLARE @UniqMajescoItem AS INT = @@uniqMajescoItem;
	DECLARE @UniqPolicyMatch AS INT = @@uniqPolicyMatch;
	DECLARE @UniqLineMatch AS INT = @@uniqLineMatch;
	DECLARE @UniqPolicy AS INT = @@uniqPolicy;
	DECLARE @UniqLine AS INT = @@uniqLine;
	DECLARE @UniqTransHead AS INT = @@uniqTransHead;
	DECLARE @UniqTransHeadDeferred AS INT = @@uniqTransHeadDeferred;
	DECLARE @UniqReceipt AS INT = @@uniqReceipt;
	DECLARE @UniqActivity AS INT = @@uniqActivity;
	DECLARE @UniqDisbursement AS INT = @@uniqDisbursement;
	DECLARE @ItemNumber AS INT = @@itemNumber;
	DECLARE @BillNumber AS INT = @@billNumber;
	DECLARE @InvoiceNumber AS INT = @@invoiceNumber;
	DECLARE @ReferNumber AS INT = @@referNumber;
	DECLARE @Exception AS VARCHAR(MAX) = @@exception;
	DECLARE @Processed AS INT = @@processed;
	DECLARE @TransactionCode AS VARCHAR(MAX) = @@transactionCode;
	DECLARE @Description AS VARCHAR(MAX) = @@description;
	DECLARE @ProcessedDate AS DATETIME = @@processedDate;

	UPDATE MajescoItem
	SET UniqPolicy = @UniqPolicy,
		UniqLine = @UniqLine,
		UniqPolicyMatch = @UniqPolicyMatch,
		UniqLineMatch = @UniqLineMatch,
		UniqTransHead = @UniqTransHead,
		UniqTransHeadDeferred = @UniqTransHeadDeferred,
		UniqActivity = @UniqActivity,
		UniqReceipt = @UniqReceipt,
		UniqDisbursement = @UniqDisbursement,
		ItemNumber = @ItemNumber,
		BillNumber = @BillNumber,
		InvoiceNumber = @InvoiceNumber,
		ReferNumber = @ReferNumber,
		Exception = @Exception,
		Processed = @Processed,
		TransactionCode = @TransactionCode,
		[Description] = @Description,
		ProcessedDate = @ProcessedDate
	WHERE UniqMajescoItem = @UniqMajescoItem;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;