BEGIN TRY

	-- Updates Transaction ID, Item Number, and Bill Number for Refunds
	UPDATE MajescoItem
	SET UniqTransHead =	m1.UniqTransHead,
		ItemNumber = m1.ItemNumber,
		BillNumber = m1.BillNumber,
		UniqPolicy = m1.UniqPolicy,
		UniqLine = m1.UniqLine
	FROM MajescoItem
	INNER JOIN MajescoItem m1
		ON m1.UniqMajescoHeader = MajescoItem.UniqMajescoHeader
			AND m1.TransactionType LIKE '%PAYMENT%'
			AND m1.TransactionType NOT LIKE '%RETURNED%'
			AND m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND MajescoItem.Processed = 0
			AND m1.UniqTransHead != -1
			AND MajescoItem.TransactionType = 'REFUND';

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;