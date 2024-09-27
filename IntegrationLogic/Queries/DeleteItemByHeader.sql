BEGIN TRY

	DECLARE @UniqMajescoHeader AS INT = @@uniqMajescoHeader;

	DELETE FROM MajescoItem
	WHERE UniqMajescoHeader = @UniqMajescoHeader;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;