BEGIN TRY

	-- Updates Policy ID and Line ID for payments
	UPDATE MajescoItem
	SET UniqPolicy = m1.UniqPolicy,
		UniqLine = m1.UniqLine
	FROM MajescoItem 
	INNER JOIN MajescoItem m1
		ON m1.TransactionType IN ('ENDORSEMENT','NEW','REINSTATEMENT')
			AND m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND MajescoItem.Processed = 0
			AND MajescoItem.UniqReceipt = -1
			AND MajescoItem.TransactionType IN ('PAYMENT','PAYMENT_TRANSFER_INTERNAL', 'PAYMENT_TRANSFER_EXTERNAL','PAYMENT_TRASNFER_INTERNAL', 'PAYMENT_TRASNFER_EXTERNAL');

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;