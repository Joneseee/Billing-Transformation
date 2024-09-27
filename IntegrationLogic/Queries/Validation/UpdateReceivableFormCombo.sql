BEGIN TRY
	
	-- Update Line Type with Receivable Code/Form No combination
	UPDATE MajescoItem
	SET LineType = CASE
						WHEN (	SELECT COUNT(*)
								FROM MajescoBondMapping
								WHERE ReceivableCode = MajescoItem.ReceivableCode
									AND FormNo = MajescoItem.FormNo) > 0 THEN (	SELECT TOP 1 LineType
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
											AND TransactionCode IS NOT NULL) > 0 THEN (	SELECT TOP 1 TransactionCode
																						FROM MajescoBondMapping
																						WHERE ReceivableCode = MajescoItem.ReceivableCode
																							AND FormNo = MajescoItem.FormNo)
								ELSE TransactionCode
							END
	WHERE Processed = 0
		AND ProcessType IN ('Bond','Cargo');

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;