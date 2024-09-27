BEGIN TRY

	SELECT *
	FROM MajescoItem
	WHERE Processed = 0
		AND UniqClient != -1
		AND UniqPolicyMatch != -1
		AND (	Exception IS NULL 
					OR Exception = '' )
		AND CarnetNumber NOT IN (	SELECT CarnetNumber
									FROM MajescoItem
									WHERE Exception IS NOT NULL 
										AND Exception != ''
										AND Processed = 0)
	UNION ALL
	SELECT *
	FROM MajescoItem
	WHERE Processed IS NULL;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;