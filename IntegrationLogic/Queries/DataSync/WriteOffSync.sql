BEGIN TRY

	-- Updates Transaction ID, Policy ID, Line ID, and Bill Number for 
	-- writeoffs with a matching Majesco Item Number
	UPDATE MajescoItem
	SET UniqTransHead =	m1.UniqTransHead,
		UniqPolicy = m1.UniqPolicy,
		UniqLine = m1.UniqLine,
		BillNumber = m1.BillNumber
	FROM MajescoItem m1
	INNER JOIN MajescoItem
		ON m1.ReceivableCode = MajescoItem.ReceivableCode
			AND m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND MajescoItem.Processed = 0
			AND m1.UniqTransHead != -1
			AND MajescoItem.TransactionType = 'WRITEOFF';

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;