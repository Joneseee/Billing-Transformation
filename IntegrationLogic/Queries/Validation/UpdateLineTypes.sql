BEGIN TRY
	
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