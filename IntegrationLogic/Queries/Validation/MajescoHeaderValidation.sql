BEGIN TRY

	-- Validate Number of Records
	UPDATE MajescoHeader
	SET Exception = CASE
						WHEN (	SELECT COUNT(*) + 1
								FROM MajescoItem
								WHERE UniqMajescoHeader = MajescoHeader.UniqMajescoHeader
									AND RecordType = 'TD'
									AND TransactionType != 'VIRTUAL_PAYMENT') != NumberOfRecords THEN 'Number of records in file does not match header.'
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
									AND RecordType = 'TD'
									AND TransactionType != 'VIRTUAL_PAYMENT') != CountOfPolicies THEN 'The total count of distinct policies does not match header.'
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
									AND TransactionType != 'VIRTUAL_PAYMENT'
									AND CreditDebit = 'DB') - (	SELECT ISNULL(SUM(Amount), 0)
																FROM MajescoItem
																WHERE UniqMajescoHeader = MajescoHeader.UniqMajescoHeader
																	AND RecordType = 'TD'
																	AND TransactionType != 'VIRTUAL_PAYMENT'
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

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;