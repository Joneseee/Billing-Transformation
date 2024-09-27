BEGIN TRY

	-- Updates Receipt ID, Transaction ID, Policy ID, Line ID for returned payments
	IF OBJECT_ID('tempdb..#tmpReturnedPaymentSync') IS NOT NULL
		DROP TABLE #tmpReturnedPaymentSync;

	SELECT	m1.UniqMajescoItem,
			m2.UniqTransHead,
			m2.UniqPolicy,
			m2.UniqLine
	INTO #tmpReturnedPaymentSync
	FROM MajescoItem m1
	INNER JOIN MajescoItem m2
		ON m1.MajescoItemNumber = m2.MajescoItemNumber
			AND m1.EntityCode = m2.EntityCode
	WHERE m2.Processed = 1
		AND m2.TransactionType IN ('PAYMENT','PAYMENT_TRANSFER_INTERNAL','PAYMENT_TRASNFER_INTERNAL','PAYMENT_TRANSFER_EXTERNAL','PAYMENT_TRASNFER_EXTERNAL')
		AND m1.TransactionType = 'RETURNED_PAYMENT'
		AND m1.Processed = 0
		AND m2.UniqMajescoItem = (	SELECT MIN(UniqMajescoItem)
									FROM MajescoItem
									WHERE TransactionType IN ('PAYMENT','PAYMENT_TRANSFER_INTERNAL','PAYMENT_TRASNFER_INTERNAL','PAYMENT_TRANSFER_EXTERNAL','PAYMENT_TRASNFER_EXTERNAL')
										AND ItemNumber != - 1
										AND MajescoItemNumber = m1.MajescoItemNumber);

	CREATE INDEX Idx_tmpReturnedPaymentSync_UniqEntity
			ON #tmpReturnedPaymentSync (UniqMajescoItem) INCLUDE (UniqTransHead, UniqPolicy, UniqLine);

	UPDATE MajescoItem
	SET ApplyToTransactionId = tmp.UniqTransHead,
		UniqPolicy = tmp.UniqPolicy,
		UniqLine = tmp.UniqLine
	FROM MajescoItem
	INNER JOIN #tmpReturnedPaymentSync tmp
		ON tmp.UniqMajescoItem = MajescoItem.UniqMajescoItem
	WHERE Processed = 0
		AND TransactionType = 'RETURNED_PAYMENT';

	DROP TABLE #tmpReturnedPaymentSync;

END TRY

BEGIN CATCH

	IF OBJECT_ID('tempdb..#tmpReturnedPaymentSync') IS NOT NULL 
			DROP TABLE #tmpReturnedPaymentSync;

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;