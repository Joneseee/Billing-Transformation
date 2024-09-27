BEGIN TRY

	UPDATE MajescoItem 
	SET UniqPolicyMatch = m1.UniqPolicyMatch,
		UniqLineMatch = m1.UniqLineMatch,
		UniqLine = m1.UniqLine,
		UniqPolicy = m1.UniqPolicy,
		BillNumber = m1.BillNumber,
		InvoiceNumber = m1.InvoiceNumber
	FROM MajescoItem
	INNER JOIN MajescoItem m1
		ON m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND m1.UniqPolicy != -1
			AND m1.UniqLine != -1
	WHERE MajescoItem.TransactionType IN ('ENDORSEMENT', 'CANCELLATION', 'REINSTATEMENT')
		AND MajescoItem.Processed = 0
		AND MajescoItem.Exception IS NULL;

	UPDATE MajescoItem
	SET UniqPolicy = l.UniqPolicy,
		UniqLine = l.UniqLine
	FROM MajescoItem
	INNER JOIN [Policy] p
		ON MajescoItem.UniqClient = p.UniqEntity
	INNER JOIN Line l
		ON p.UniqPolicy = l.UniqPolicy
	INNER JOIN CdPolicyLineType lt
		ON l.UniqCdPolicyLineType = lt.UniqCdPolicyLineType
	WHERE p.PolicyNumber = MajescoItem.CarnetNumber
		AND p.EffectiveDate = MajescoItem.EffectiveDate
		AND p.ExpirationDate = MajescoItem.ExpirationDate
		AND lt.CdPolicyLineTypeCode = MajescoItem.LineType
		AND MajescoItem.TransactionType IN ('ENDORSEMENT', 'CANCELLATION', 'REINSTATEMENT')
		AND MajescoItem.UniqPolicy = -1
		AND MajescoItem.UniqLine = -1
		AND MajescoItem.UniqPolicyMatch != -1
		AND MajescoItem.UniqLineMatch != -1
		AND MajescoItem.Processed = 0
		AND MajescoItem.Exception IS NULL;

	UPDATE MajescoItem
	SET UniqPolicy = l.UniqPolicy,
		UniqLine = l.UniqLine
	FROM MajescoItem
	INNER JOIN [Policy] p
		ON MajescoItem.UniqClient = p.UniqEntity
	INNER JOIN Line l
		ON p.UniqPolicy = l.UniqPolicy
	INNER JOIN CdPolicyLineType lt
		ON l.UniqCdPolicyLineType = lt.UniqCdPolicyLineType
	WHERE LTRIM(RTRIM(l.SiteID)) = LTRIM(RTRIM(MajescoItem.LocationId))
		AND p.EffectiveDate <= MajescoItem.EffectiveDate
		AND p.ExpirationDate >= MajescoItem.EffectiveDate
		AND lt.CdPolicyLineTypeCode = MajescoItem.LineType
		AND l.UniqEntityCompanyIssuing = MajescoItem.UniqCompany
		AND MajescoItem.TransactionType IN ('ENDORSEMENT', 'CANCELLATION', 'REINSTATEMENT')
		AND MajescoItem.UniqPolicy = -1
		AND MajescoItem.UniqLine = -1
		AND MajescoItem.UniqPolicyMatch != -1
		AND MajescoItem.UniqLineMatch != -1
		AND MajescoItem.Processed = 0
		AND MajescoItem.Exception IS NULL;

	UPDATE MajescoItem
	SET UniqPolicy = l.UniqPolicy,
		UniqLine = l.UniqLine
	FROM MajescoItem
	INNER JOIN [Policy] p
		ON MajescoItem.UniqClient = p.UniqEntity
	INNER JOIN Line l
		ON p.UniqPolicy = l.UniqPolicy
	INNER JOIN CdPolicyLineType lt
		ON l.UniqCdPolicyLineType = lt.UniqCdPolicyLineType
	WHERE LTRIM(RTRIM(l.SiteID)) = LTRIM(RTRIM(MajescoItem.LocationId))
		AND p.EffectiveDate <= MajescoItem.EffectiveDate
		AND p.ExpirationDate >= MajescoItem.EffectiveDate
		AND lt.CdPolicyLineTypeCode = MajescoItem.LineType
		AND MajescoItem.TransactionType IN ('ENDORSEMENT', 'CANCELLATION', 'REINSTATEMENT')
		AND MajescoItem.UniqPolicy = -1
		AND MajescoItem.UniqLine = -1
		AND MajescoItem.UniqPolicyMatch != -1
		AND MajescoItem.UniqLineMatch != -1
		AND MajescoItem.Processed = 0
		AND MajescoItem.Exception IS NULL;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;