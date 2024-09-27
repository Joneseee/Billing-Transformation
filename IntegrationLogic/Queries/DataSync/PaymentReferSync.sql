BEGIN TRY
	
	-- Updates payment receipt with reference number
	UPDATE MajescoItem
	SET ReferNumber = (	SELECT ReferNumber
						FROM Receipt
						WHERE UniqReceipt = MajescoItem.UniqReceipt)
	WHERE UniqReceipt != -1
		AND Processed = 0
		AND TransactionType LIKE '%PAYMENT%'
		AND TransactionType NOT LIKE '%RETURNED%';

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;