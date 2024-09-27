BEGIN TRY
	
	-- Updates Transaction ID, Item Number, Bill Number, Disbursement ID, Policy ID, and Line ID
	-- for void refund
	UPDATE MajescoItem
	SET UniqTransHead =	m1.UniqTransHead,
		ItemNumber = m1.ItemNumber,
		BillNumber = m1.BillNumber,
		UniqDisbursement = m1.UniqDisbursement,
		UniqPolicy = m1.UniqPolicy,
		UniqLine = m1.UniqLine
	FROM MajescoItem 
	INNER JOIN MajescoItem m1
		ON m1.TransactionType = 'REFUND'
			AND m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND MajescoItem.Processed = 0
			AND MajescoItem.UniqDisbursement = -1
			AND m1.UniqDisbursement != -1
			AND MajescoItem.TransactionType = 'VOID_REFUND';

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;