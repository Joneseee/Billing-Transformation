BEGIN TRY

	-- Update Company Mapping for new items
	UPDATE MajescoCompanyMapping
	SET UniqCompany = (	SELECT UniqEntity
						FROM Company
						WHERE LookupCode = MajescoCompanyMapping.LookupCode)
	FROM MajescoCompanyMapping
	WHERE 0 < (	SELECT COUNT(*)
				FROM Company
				WHERE LookupCode = MajescoCompanyMapping.LookupCode)
		AND (UniqCompany = -1 OR UniqCompany IS NULL);

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;