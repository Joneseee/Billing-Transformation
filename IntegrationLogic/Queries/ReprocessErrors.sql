BEGIN TRY
	IF OBJECT_ID('tempdb..#tmpReprocess') IS NOT NULL DROP TABLE #tmpReprocess;

	SELECT DISTINCT EntityCode,
					CarnetNumber,
					EffectiveDate,
					ExpirationDate,
					LocationId
	INTO #tmpReprocess
	FROM MajescoItem
	WHERE Processed = 0;

	UPDATE MajescoItem
	SET    UniqClient = NULL,
		   UniqPolicyMatch = -1,
		   UniqLineMatch = -1,
		   UniqTransHead = -1,
		   UniqTransHeadDeferred = NULL,
		   UniqPolicy = -1,
		   UniqLine = -1,
		   UniqActivity = -1,
		   UniqReceipt = -1,
		   UniqDisbursement = -1,
		   ItemNumber = -1,
		   BillNumber = -1,
		   InvoiceNumber = -1,
		   ReferNumber = -1,
		   ApplyToItemNumber = -1,
		   ApplyToTransactionId = -1,
		   ApplyToDeferredTransactionId = NULL,
		   ApplyToTransactionCode = '',
		   Exception = NULL,
		   Processed = 0
	FROM MajescoItem
	INNER JOIN #tmpReprocess tmp
		ON MajescoItem.CarnetNumber = tmp.CarnetNumber
			AND MajescoItem.EntityCode = tmp.EntityCode
			AND MajescoItem.EffectiveDate = tmp.EffectiveDate
			AND MajescoItem.ExpirationDate = tmp.ExpirationDate
			AND MajescoItem.LocationId = tmp.LocationId
	WHERE MajescoItem.Processed = 0
		AND MajescoItem.UniqMajescoItem NOT IN (	SELECT UniqMajescoItem
													FROM MajescoItem
													WHERE Exception LIKE '%Accounting month selected is locked%')
		AND MajescoItem.UniqMajescoItem NOT IN (	SELECT UniqMajescoItem
													FROM MajescoItem
													WHERE Exception LIKE '%Bill number not valid for the agency on the policy%');

	DROP TABLE #tmpReprocess;
	UPDATE MajescoItem
	SET Exception = NULL
	WHERE Exception LIKE '%Accounting month selected is locked%';

	UPDATE MajescoItem
	SET Exception = NULL,
		UniqTransHead = -1,
		UniqTransHeadDeferred = NULL,
		ItemNumber = -1,
		BillNumber = -1,
		InvoiceNumber = -1,
		ReferNumber = -1
	WHERE Exception LIKE '%Bill number not valid for the agency on the policy%'

END TRY

BEGIN CATCH

	IF OBJECT_ID('tempdb..#tmpReprocess') IS NOT NULL DROP TABLE #tmpReprocess;

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH