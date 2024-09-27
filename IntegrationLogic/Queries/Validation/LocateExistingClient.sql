BEGIN TRY

	-- Locate Client ID
	UPDATE MajescoItem
	SET UniqClient = (	SELECT TOP 1 UniqEntity
						FROM Client
						WHERE LookupCode = SUBSTRING(MajescoItem.CustomerEntityCode, 1, 10))
	WHERE UniqMajescoHeader IN (	SELECT UniqMajescoHeader
									FROM MajescoHeader
									WHERE Validated = 1
										AND Processed = 0
										AND Exception IS NULL)
		AND Processed = 0
		AND Exception IS NULL
		AND (UniqClient IS NULL OR UniqClient = -1);

	-- Locate Client ID
	UPDATE MajescoItem
	SET UniqClient = (	SELECT TOP 1 UniqEntity
						FROM Client
						WHERE LookupCode = SUBSTRING(MajescoItem.EntityCode, 1, 10))
	WHERE UniqMajescoHeader IN (	SELECT UniqMajescoHeader
									FROM MajescoHeader
									WHERE Validated = 1
										AND Processed = 0
										AND Exception IS NULL)
		AND Processed = 0
		AND Exception IS NULL
		AND (UniqClient IS NULL OR UniqClient = -1);

	-- Update item validation when Client ID is not located
	UPDATE MajescoItem
	SET Exception = 'Client could not be located with the lookup code provided.'
	WHERE UniqMajescoHeader IN (	SELECT UniqMajescoHeader
									FROM MajescoHeader
									WHERE Validated = 1
										AND Processed = 0
										AND Exception IS NULL)
		AND Processed = 0
		AND Exception IS NULL
		AND UniqClient = -1;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;