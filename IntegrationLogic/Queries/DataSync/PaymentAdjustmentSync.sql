BEGIN TRY

	-- Updates Transaction ID, Item Number, Bill Number, Receipt ID, Policy ID, and Line ID
	-- for payment adjustment
	UPDATE MajescoItem
	SET UniqTransHead =	m1.UniqTransHead,
		ItemNumber = m1.ItemNumber,
		BillNumber = m1.BillNumber,
		UniqReceipt = m1.UniqReceipt,
		UniqPolicy = m1.UniqPolicy,
		UniqLine = m1.UniqLine
	FROM MajescoItem m1
	INNER JOIN MajescoItem
		ON m1.TransactionType IN ('PAYMENT','PAYMENT_TRANSFER_INTERNAL', 'PAYMENT_TRANSFER_EXTERNAL','PAYMENT_TRASNFER_INTERNAL', 'PAYMENT_TRASNFER_EXTERNAL')
			AND m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND MajescoItem.Processed = 0
			AND MajescoItem.TransactionType = 'PAYMENT_ADJUSTMENT';

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;