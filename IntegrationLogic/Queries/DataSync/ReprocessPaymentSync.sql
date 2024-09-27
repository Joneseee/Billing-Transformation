BEGIN TRY

	-- Update has balance of zero
	UPDATE MajescoItem
	SET Exception = NULL
	WHERE Exception LIKE '%Error creating receipt%' OR Exception LIKE '%Error updating receipt%'
		AND Processed = 0;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;