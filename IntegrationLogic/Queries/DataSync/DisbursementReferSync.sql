BEGIN TRY

	-- Updates disbursement with corresponding reference number
	UPDATE MajescoItem
	SET ReferNumber = (	SELECT ReferNumber
						FROM Disbursement
						WHERE UniqDisbursement = MajescoItem.UniqDisbursement)
	WHERE UniqDisbursement != -1
		AND Processed = 0
		AND TransactionType LIKE '%REFUND%';

	UPDATE MajescoItem
	SET ReferNumber = d.ReferNumber
	FROM Disbursement d
	INNER JOIN MajescoItem
		ON d.UniqDisbursementVoid = MajescoItem.UniqDisbursement
			AND MajescoItem.UniqDisbursement != -1
			AND MajescoItem.Processed = 0;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;