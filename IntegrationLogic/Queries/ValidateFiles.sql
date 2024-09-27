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
		AND UniqCompany = -1;

	-- Update Line Type with Receivable Code/Form No combination
	UPDATE MajescoItem
	SET LineType = CASE
						WHEN (	SELECT COUNT(*)
								FROM MajescoBondMapping
								WHERE ReceivableCode = MajescoItem.ReceivableCode
									AND FormNo = MajescoItem.FormNo) > 0 THEN (	SELECT LineType
																				FROM MajescoBondMapping
																				WHERE ReceivableCode = MajescoItem.ReceivableCode
																					AND FormNo = MajescoItem.FormNo)
						ELSE LineType
					END,
		ProcessType =	CASE
							WHEN (	SELECT COUNT(*)
									FROM MajescoBondMapping
									WHERE ReceivableCode = MajescoItem.ReceivableCode
										AND FormNo = MajescoItem.FormNo) > 0 THEN 'Bond'
							ELSE ProcessType
						END
	WHERE Processed = 0;

	-- Update Transaction Code with Receivable Code/Form No combination
	UPDATE MajescoItem
	SET TransactionCode =	CASE
								WHEN (	SELECT COUNT(*)
										FROM MajescoBondMapping
										WHERE ReceivableCode = MajescoItem.ReceivableCode
											AND FormNo = MajescoItem.FormNo
											AND TransactionCode != ''
											AND TransactionCode IS NOT NULL) > 0 THEN (	SELECT TransactionCode
																						FROM MajescoBondMapping
																						WHERE ReceivableCode = MajescoItem.ReceivableCode
																							AND FormNo = MajescoItem.FormNo)
								ELSE	CASE
											WHEN TransactionType = 'NEW' THEN 'NEWB'
											WHEN TransactionType = 'CANCELLATION' THEN 'CANC'
											ELSE 'ENDT'
										END
							END
	WHERE Processed = 0
		AND ProcessType IN ('Bond','Cargo');

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
					WHERE UnderwritingCompanyCode = MajescoItem.UnderwritingCompanyCode)
		AND ProcessType IN ('Bond','Cargo');

	-- Validate Number of Records
	UPDATE MajescoHeader
	SET Exception = CASE
						WHEN (	SELECT COUNT(*) + 1
								FROM MajescoItem
								WHERE UniqMajescoHeader = MajescoHeader.UniqMajescoHeader
									AND RecordType = 'TD') != NumberOfRecords THEN 'Number of records in file does not match header.'
						ELSE NULL
					END
	WHERE Exception IS NULL
		OR Exception = '';

	-- Validate Count of Policies
	UPDATE MajescoHeader
	SET Exception = CASE
						WHEN (	SELECT COUNT(DISTINCT CarnetNumber)
								FROM MajescoItem
								WHERE UniqMajescoHeader = MajescoHeader.UniqMajescoHeader
									AND RecordType = 'TD') != CountOfPolicies THEN 'The total count of distinct policies does not match header.'
						ELSE NULL
					END
	WHERE Exception IS NULL
		OR Exception = '';

	-- Validate Transaction Total
	UPDATE MajescoHeader
	SET Exception = CASE
						WHEN (	SELECT ISNULL(SUM(Amount), 0)
								FROM MajescoItem
								WHERE UniqMajescoHeader = MajescoHeader.UniqMajescoHeader
									AND RecordType = 'TD'
									AND CreditDebit = 'DB') - (	SELECT ISNULL(SUM(Amount), 0)
																FROM MajescoItem
																WHERE UniqMajescoHeader = MajescoHeader.UniqMajescoHeader
																	AND RecordType = 'TD'
																	AND CreditDebit = 'CR') != CASE WHEN CreditDebit = 'CR' THEN (TotalTransactionAmount * -1) ELSE TotalTransactionAmount END THEN 'The total transaction amount does not match header.'
						ELSE NULL
					END
	WHERE Exception IS NULL
		OR Exception = '';

	-- Update header validation
	UPDATE MajescoHeader
	SET Validated = 1
	WHERE Validated = 0
		AND Processed = 0
		AND Exception IS NULL;

	-- Locate Client ID by Customer Entity Code
	UPDATE MajescoItem
	SET UniqClient = (	SELECT TOP 1 UniqEntity
						FROM Client
						WHERE LookupCode = SUBSTRING(MajescoItem.CustomerEntityCode, 1, 10))
	WHERE UniqMajescoHeader IN (	SELECT UniqMajescoHeader
									FROM MajescoHeader
									WHERE Validated = 1
										AND Processed = 0
										AND Exception IS NULL)
		AND CustomerEntityCode IS NOT NULL
		AND Processed = 0
		AND Exception IS NULL;

	-- Locate Client ID by Entity Code
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
		AND UniqClient = -1;

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

	-- Locate existing Policy/Line - Bond / Cargo Only
	UPDATE MajescoItem
	SET UniqPolicyMatch = l.UniqPolicy,
		UniqLineMatch = l.UniqLine
	FROM Line l
	INNER JOIN [Policy] p
		ON p.UniqPolicy = l.UniqPolicy
	INNER JOIN CdPolicyLineType lt
		ON l.UniqCdPolicyLineType = lt.UniqCdPolicyLineType
	INNER JOIN MajescoItem
		ON MajescoItem.LocationId = l.SiteID
			AND p.UniqEntity = MajescoItem.UniqClient
			AND lt.CdPolicyLineTypeCode = MajescoItem.LineType
	WHERE UniqMajescoHeader IN (	SELECT UniqMajescoHeader
									FROM MajescoHeader
									WHERE Validated = 1
										AND Processed = 0
										AND Exception IS NULL)
		AND Processed = 0
		AND Exception IS NULL
		AND UniqClient != -1
		AND UniqPolicyMatch = -1
		AND UniqLineMatch = -1
		AND CONVERT(DATE, l.EffectiveDate) <= CONVERT(DATE, MajescoItem.EffectiveDate)
		AND CONVERT(DATE, l.ExpirationDate) >= CONVERT(DATE, MajescoItem.EffectiveDate)
		AND l.UniqPolicy IN (	SELECT DISTINCT l.UniqPolicy
								FROM Line l
								INNER JOIN LineAgencyDefined lad
									ON l.UniqLine = lad.UniqLine
								WHERE l.SiteID = MajescoItem.LocationId
									AND ( CategoryCode LIKE '%Bond Flag%'
											OR CategoryCode LIKE '%Cargo Flag%'));

	-- Locate existing Policy/Line - ALL
	UPDATE MajescoItem
	SET UniqPolicyMatch = l.UniqPolicy,
		UniqLineMatch = l.UniqLine
	FROM Line l
	INNER JOIN [Policy] p
		ON p.UniqPolicy = l.UniqPolicy
	INNER JOIN CdPolicyLineType lt
		ON l.UniqCdPolicyLineType = lt.UniqCdPolicyLineType
	INNER JOIN MajescoItem
		ON MajescoItem.LocationId = l.SiteID
			AND p.UniqEntity = MajescoItem.UniqClient
			AND lt.CdPolicyLineTypeCode = MajescoItem.LineType
	WHERE UniqMajescoHeader IN (	SELECT UniqMajescoHeader
									FROM MajescoHeader
									WHERE Validated = 1
										AND Processed = 0
										AND Exception IS NULL)
		AND Processed = 0
		AND Exception IS NULL
		AND UniqClient != -1
		AND UniqPolicyMatch = -1
		AND UniqLineMatch = -1
		AND l.UniqPolicy IN (	SELECT DISTINCT l.UniqPolicy
								FROM Line l
								INNER JOIN LineAgencyDefined lad
									ON l.UniqLine = lad.UniqLine
								WHERE l.SiteID = MajescoItem.LocationId
									AND ( CategoryCode LIKE '%Carnet Flag%' 
											OR CategoryCode LIKE '%Bond Flag%'
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

	-- Update STAX and CTAX Line Types
	UPDATE MajescoItem
	SET LineType = pt.CdPolicyLineTypeCode,
		ProcessType =CASE
							WHEN pt.CdPolicyLineTypeCode = 'CARN' THEN 'Carnet'
							WHEN pt.CdPolicyLineTypeCode = 'CRGO' THEN 'Cargo'
							ELSE 'Bond'
						END
	FROM MajescoItem
	INNER JOIN [Policy] p
		ON MajescoItem.UniqPolicyMatch = p.UniqPolicy
			AND MajescoItem.ReceivableCode IN ('STAX','CTAX')
	INNER JOIN CdPolicyLineType pt
		ON pt.UniqCdPolicyLineType = p.UniqCdPolicyLineType
	WHERE MajescoItem.Processed = 0;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;