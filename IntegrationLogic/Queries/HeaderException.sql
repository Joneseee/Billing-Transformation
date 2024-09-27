BEGIN TRY

	DECLARE @UniqMajescoHeader AS INT = @@uniqMajescoHeader;
	DECLARE @HeaderException AS VARCHAR(MAX) = @@headerException;

	UPDATE MajescoHeader
	SET Exception = @HeaderException
	WHERE UniqMajescoHeader = @UniqMajescoHeader;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;