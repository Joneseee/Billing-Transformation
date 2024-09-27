BEGIN TRY

	-- Update UniqCompany with mapping
	UPDATE MajescoItem
	SET UniqCompany = (	SELECT UniqCompany
						FROM MajescoCompanyMapping
						WHERE UnderwritingCompanyCode = MajescoItem.UnderwritingCompanyCode),
		CompanyLookupCode = (	SELECT LookupCode
								FROM MajescoCompanyMapping
								WHERE UnderwritingCompanyCode = MajescoItem.UnderwritingCompanyCode)
	WHERE Processed = 0
		AND 0 < (	SELECT COUNT(*)
					FROM MajescoCompanyMapping
					WHERE UnderwritingCompanyCode = MajescoItem.UnderwritingCompanyCode);

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;