BEGIN TRY

	DECLARE @ApplyToTransactionId AS INT = @@applyToTransactionId;

	SELECT TOP 1	ttf.UniqVendor,
					ttf.UniqContactName,
					st.CdStateCode,
					ttf.TaxableAmount
	FROM TransTaxFee ttf
	INNER JOIN CdState st
		ON ttf.UniqCdState = st.UniqCdState
	WHERE UniqTransHead IN (SELECT UniqTransHead
							FROM TransHead
							WHERE UniqTaxableItem = @ApplyToTransactionId)
	ORDER BY UniqTransTaxFee DESC;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;