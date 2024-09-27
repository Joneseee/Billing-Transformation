BEGIN TRY
	
	-- Locate existing Policy/Line including issuing company match - Bond / Cargo Only
	UPDATE MajescoItem
	SET UniqPolicyMatch = l.UniqPolicy,
		UniqLineMatch = l.UniqLine
	FROM MajescoItem
	INNER JOIN [Policy] p
		ON MajescoItem.UniqClient = p.UniqEntity
	INNER JOIN Line l
		ON l.UniqPolicy = p.UniqPolicy
	INNER JOIN CdPolicyLineType lt
		ON l.UniqCdPolicyLineType = lt.UniqCdPolicyLineType
	WHERE RTRIM(LTRIM(MajescoItem.LocationId)) = RTRIM(LTRIM(l.SiteID))
		AND lt.CdPolicyLineTypeCode = MajescoItem.LineType
		AND MajescoItem.Processed = 0
		AND MajescoItem.Exception IS NULL
		AND MajescoItem.UniqClient != -1
		AND MajescoItem.UniqPolicyMatch = -1
		AND CONVERT(DATE, l.EffectiveDate) <= CONVERT(DATE, MajescoItem.EffectiveDate)
		AND CONVERT(DATE, l.ExpirationDate) >= CONVERT(DATE, MajescoItem.EffectiveDate)
		AND MajescoItem.UniqMajescoHeader IN (	SELECT UniqMajescoHeader
												FROM MajescoHeader
												WHERE Validated = 1
													AND Processed = 0
													AND Exception IS NULL)
		AND l.UniqEntityCompanyIssuing = MajescoItem.UniqCompany
		AND ( MajescoItem.UniqCompany IS NOT NULL 
				OR MajescoItem.UniqCompany != -1)
		AND l.UniqPolicy IN (	SELECT DISTINCT l.UniqPolicy
								FROM Line l
								INNER JOIN LineAgencyDefined lad
									ON l.UniqLine = lad.UniqLine
								WHERE RTRIM(LTRIM(MajescoItem.LocationId)) = RTRIM(LTRIM(l.SiteID))
									AND ( CategoryCode LIKE '%Bond Flag%'
											OR CategoryCode LIKE '%Bonds Flag%'
											OR CategoryCode LIKE '%Cargo Flag%'));

	-- Locate existing Policy/Line without issuing company match - Bond / Cargo Only
	UPDATE MajescoItem
	SET UniqPolicyMatch = l.UniqPolicy,
		UniqLineMatch = l.UniqLine
	FROM MajescoItem
	INNER JOIN [Policy] p
		ON MajescoItem.UniqClient = p.UniqEntity
	INNER JOIN Line l
		ON l.UniqPolicy = p.UniqPolicy
	INNER JOIN CdPolicyLineType lt
		ON l.UniqCdPolicyLineType = lt.UniqCdPolicyLineType
	WHERE RTRIM(LTRIM(MajescoItem.LocationId)) = RTRIM(LTRIM(l.SiteID))
		AND lt.CdPolicyLineTypeCode = MajescoItem.LineType
		AND MajescoItem.Processed = 0
		AND MajescoItem.Exception IS NULL
		AND MajescoItem.UniqClient != -1
		AND MajescoItem.UniqPolicyMatch = -1
		AND CONVERT(DATE, l.EffectiveDate) <= CONVERT(DATE, MajescoItem.EffectiveDate)
		AND CONVERT(DATE, l.ExpirationDate) >= CONVERT(DATE, MajescoItem.EffectiveDate)
		AND MajescoItem.UniqMajescoHeader IN (	SELECT UniqMajescoHeader
												FROM MajescoHeader
												WHERE Validated = 1
													AND Processed = 0
													AND Exception IS NULL)
		AND l.UniqPolicy IN (	SELECT DISTINCT l.UniqPolicy
								FROM Line l
								INNER JOIN LineAgencyDefined lad
									ON l.UniqLine = lad.UniqLine
								WHERE RTRIM(LTRIM(MajescoItem.LocationId)) = RTRIM(LTRIM(l.SiteID))
									AND ( CategoryCode LIKE '%Bond Flag%'
											OR CategoryCode LIKE '%Bonds Flag%'
											OR CategoryCode LIKE '%Cargo Flag%'));

	-- Locate existing Policy/Line - ALL
	UPDATE MajescoItem
	SET UniqPolicyMatch = l.UniqPolicy,
		UniqLineMatch = l.UniqLine
	FROM MajescoItem
	INNER JOIN [Policy] p
		ON MajescoItem.UniqClient = p.UniqEntity
	INNER JOIN Line l
		ON l.UniqPolicy = p.UniqPolicy
	INNER JOIN CdPolicyLineType lt
		ON l.UniqCdPolicyLineType = lt.UniqCdPolicyLineType
	WHERE RTRIM(LTRIM(MajescoItem.LocationId)) = RTRIM(LTRIM(l.SiteID))
		AND lt.CdPolicyLineTypeCode = MajescoItem.LineType
		AND MajescoItem.Processed = 0
		AND MajescoItem.Exception IS NULL
		AND MajescoItem.UniqClient != -1
		AND MajescoItem.UniqPolicyMatch = -1
		AND MajescoItem.UniqMajescoHeader IN (	SELECT UniqMajescoHeader
												FROM MajescoHeader
												WHERE Validated = 1
													AND Processed = 0
													AND Exception IS NULL)
		AND l.UniqPolicy IN (	SELECT DISTINCT l.UniqPolicy
								FROM Line l
								INNER JOIN LineAgencyDefined lad
									ON l.UniqLine = lad.UniqLine
								WHERE RTRIM(LTRIM(MajescoItem.LocationId)) = RTRIM(LTRIM(l.SiteID))
									AND ( CategoryCode LIKE '%Carnet Flag%' 
											OR CategoryCode LIKE '%Bond Flag%'
											OR CategoryCode LIKE '%Bonds Flag%'
											OR CategoryCode LIKE '%Cargo Flag%'));

	-- Update item validation when existing policy is not located
	UPDATE MajescoItem
	SET Exception = 'Existing policy/line could not be located by the Location ID provided.'
	WHERE Processed = 0
		AND Exception IS NULL
		AND TransactionType = 'NEW'
		AND UniqClient != -1
		AND (UniqPolicyMatch = -1
				OR UniqLineMatch = -1);

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;