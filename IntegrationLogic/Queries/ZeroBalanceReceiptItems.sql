BEGIN TRY

	SELECT *
	FROM MajescoItem
	WHERE Processed = 0
		AND Exception LIKE '%has a balance of $0.00.%';

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;