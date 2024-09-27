BEGIN TRY

	-- Updates ApplyToTransactionId, ApplyToDeferredTransactionId, DeferredAmount, ApplyToTransactionCode,
	-- ApplyToItemNumber, and ApplyToEffectiveDate for payments
	UPDATE MajescoItem
	SET UniqPolicy = m1.UniqPolicy,
		UniqLine = m1.UniqLine,
		ApplyToTransactionId = m1.UniqTransHead,
		ApplyToDeferredTransactionId = m1.UniqTransHeadDeferred,
		DeferredAmount = m1.DeferredAmount,
		ApplyToTransactionCode = m1.TransactionCode,
		ApplyToItemNumber = m1.ItemNumber,
		ApplyToEffectiveDate = m1.EffectiveDate
	FROM MajescoItem 
	INNER JOIN MajescoItem m1
		ON m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND m1.TransactionType IN ('NEW','ENDORSEMENT','REINSTATEMENT')
			AND MajescoItem.TransactionType LIKE '%PAYMENT%'
			AND MajescoItem.TransactionType != 'RETURNED_PAYMENT'
			AND m1.UniqTransHead != -1;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;