BEGIN TRY

	-- Updates Majesco Header to processed when all 
	-- associated Majesco Items have been processed
	UPDATE MajescoHeader
	SET Processed =	CASE
						WHEN (	SELECT COUNT(*)
								FROM MajescoItem
								WHERE UniqMajescoHeader = MajescoHeader.UniqMajescoHeader
									AND Processed = 0) = 0 THEN 1
						ELSE 0
					END;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;