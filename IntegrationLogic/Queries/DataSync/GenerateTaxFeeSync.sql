BEGIN TRY
	
	DECLARE @TransactionCodeString AS VARCHAR(MAX) = @@transactionCodeString;

	-- Updates ApplyToTransactionId for processing taxes and fees
	UPDATE MajescoItem
	SET ApplyToTransactionId =	CASE
									WHEN (	SELECT COUNT(*)
											FROM MajescoItem
											WHERE CarnetNumber = MajescoItem.CarnetNumber
												AND LocationId = MajescoItem.LocationId
												AND TransactionType = 'NEW'
												AND EffectiveDate = MajescoItem.EffectiveDate
												AND UniqTransHead NOT IN (-1,0)
												AND Processed = 1) = 0 THEN -1
									ELSE (	SELECT TOP 1 UniqTransHead
											FROM MajescoItem
											WHERE CarnetNumber = MajescoItem.CarnetNumber
												AND LocationId = MajescoItem.LocationId
												AND TransactionType = 'NEW'
												AND EffectiveDate = MajescoItem.EffectiveDate
												AND UniqTransHead NOT IN (-1,0)
												AND Processed = 1)
								END
	WHERE TransactionType IN ('ENDORSEMENT','NEW','REINSTATEMENT')
		AND Processed = 0
		AND TransactionCode IN (SELECT VALUE
								FROM STRING_SPLIT(@TransactionCodeString,';'));

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;