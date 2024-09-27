BEGIN TRY

	UPDATE MajescoHeader
	SET Processed =	CASE
						WHEN (	SELECT COUNT(*)
								FROM MajescoItem
								WHERE UniqMajescoHeader = MajescoHeader.UniqMajescoHeader
									AND Processed = 0) = 0 THEN 1
						ELSE 0
					END;

	UPDATE MajescoItem
	SET ItemNumber = (	SELECT ItemNumber
						FROM TransHead
						WHERE UniqTransHead = MajescoItem.UniqTransHead)
	WHERE UniqTransHead != -1;

	UPDATE MajescoItem
	SET ItemNumber = (	SELECT th.ItemNumber
						FROM TransHead th
						INNER JOIN TransDetail td
							ON th.UniqTransHead = td.UniqTransHead
						WHERE td.ItemAmount != -1
							AND td.UniqReceipt = MajescoItem.UniqReceipt
							AND td.TransDetailNumber = 1),
		UniqTransHead = (	SELECT th.UniqTransHead
							FROM TransHead th
							INNER JOIN TransDetail td
								ON th.UniqTransHead = td.UniqTransHead
							WHERE td.ItemAmount != -1
								AND td.UniqReceipt = MajescoItem.UniqReceipt
								AND td.TransDetailNumber = 1),
		BillNumber = (	SELECT th.BillNumber
						FROM TransHead th
						INNER JOIN TransDetail td
							ON th.UniqTransHead = td.UniqTransHead
						WHERE td.ItemAmount != -1
							AND td.UniqReceipt = MajescoItem.UniqReceipt
							AND td.TransDetailNumber = 1)
	WHERE UniqReceipt != -1
		AND Processed IN (0,3);

	UPDATE MajescoItem
	SET BillNumber = -1
	WHERE BillNumber IS NULL;

	UPDATE MajescoItem
	SET ItemNumber = -1
	WHERE ItemNumber IS NULL;

	UPDATE MajescoItem
	SET UniqTransHead = -1
	WHERE UniqTransHead IS NULL;

	UPDATE MajescoItem
	SET UniqTransHead =	m1.UniqTransHead,
		UniqPolicy = m1.UniqPolicy,
		UniqLine = m1.UniqLine,
		BillNumber = m1.BillNumber
	FROM MajescoItem m1
	INNER JOIN MajescoItem
		ON m1.ReceivableCode = MajescoItem.ReceivableCode
			AND m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND MajescoItem.Processed = 0
			AND m1.UniqTransHead != -1
			AND MajescoItem.TransactionType = 'WRITEOFF';

	UPDATE MajescoItem
	SET UniqTransHead =	m1.UniqTransHead,
		ItemNumber = m1.ItemNumber,
		BillNumber = m1.BillNumber
	FROM MajescoItem m1
	INNER JOIN MajescoItem
		ON m1.UniqMajescoHeader = MajescoItem.UniqMajescoHeader
			AND m1.TransactionType LIKE '%PAYMENT%'
			AND m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND MajescoItem.Processed = 0
			AND m1.UniqTransHead != -1
			AND MajescoItem.TransactionType = 'REFUND';

	UPDATE MajescoItem
	SET ReferNumber = (	SELECT ReferNumber
						FROM Receipt
						WHERE UniqReceipt = MajescoItem.UniqReceipt)
	WHERE UniqReceipt != -1
		AND Processed = 0
		AND TransactionType LIKE '%PAYMENT%';


	UPDATE MajescoItem
	SET ReferNumber = (	SELECT ReferNumber
						FROM Disbursement
						WHERE UniqDisbursement = MajescoItem.UniqDisbursement)
	WHERE UniqDisbursement != -1
		AND Processed = 0
		AND TransactionType LIKE '%REFUND%';

	UPDATE MajescoItem
	SET UniqTransHead =	m1.UniqTransHead,
		ItemNumber = m1.ItemNumber,
		BillNumber = m1.BillNumber,
		UniqDisbursement = m1.UniqDisbursement,
		UniqPolicy = m1.UniqPolicy,
		UniqLine = m1.UniqLine
	FROM MajescoItem m1
	INNER JOIN MajescoItem
		ON m1.TransactionType = 'REFUND'
			AND m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND MajescoItem.Processed = 0
			AND MajescoItem.UniqDisbursement = -1
			AND m1.UniqDisbursement != -1
			AND MajescoItem.TransactionType = 'VOID_REFUND';

	UPDATE MajescoItem
	SET UniqTransHead =	m1.UniqTransHead,
		ItemNumber = m1.ItemNumber,
		BillNumber = m1.BillNumber,
		UniqReceipt = m1.UniqReceipt,
		UniqPolicy = m1.UniqPolicy,
		UniqLine = m1.UniqLine
	FROM MajescoItem m1
	INNER JOIN MajescoItem
		ON m1.TransactionType IN ('PAYMENT','PAYMENT_TRANSFER_INTERNAL', 'PAYMENT_TRANSFER_EXTERNAL','PAYMENT_TRASNFER_INTERNAL', 'PAYMENT_TRASNFER_EXTERNAL')
			AND m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND MajescoItem.Processed = 0
			--AND MajescoItem.UniqDisbursement = -1
			--AND m1.UniqDisbursement != -1
			AND MajescoItem.TransactionType = 'PAYMENT_ADJUSTMENT';

	UPDATE MajescoItem
	SET ReferNumber = d.ReferNumber,
		UniqDisbursement = d.UniqDisbursement
	FROM Disbursement d
	INNER JOIN MajescoItem
		ON d.UniqDisbursementVoid = MajescoItem.UniqDisbursement
			AND MajescoItem.UniqDisbursement != -1
			AND MajescoItem.Processed = 0;

	UPDATE MajescoItem
	SET ApplyToTransactionId = m1.UniqTransHead,
		ApplyToDeferredTransactionId = m1.UniqTransHeadDeferred,
		DeferredAmount = m1.DeferredAmount,
		ApplyToTransactionCode = m1.TransactionCode,
		ApplyToItemNumber = m1.ItemNumber,
		ApplyToEffectiveDate = m1.EffectiveDate
	FROM MajescoItem m1
	INNER JOIN MajescoItem
		ON m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND m1.TransactionType IN ('NEW','ENDORSEMENT')
			AND MajescoItem.TransactionType LIKE '%PAYMENT%'
			AND m1.UniqTransHead != -1;

	-- Data Sync Returned Payments
	UPDATE MajescoItem
	SET UniqReceipt = m1.UniqReceipt,
		UniqTransHead = m1.UniqTransHead,
		UniqPolicy = m1.UniqPolicy,
		UniqLine = m1.UniqLine,
		ItemNumber = m1.ItemNumber,
		BillNumber = m1.BillNumber,
		ReferNumber = m1.ReferNumber
	FROM MajescoItem m1
	INNER JOIN MajescoItem
		ON m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
	WHERE MajescoItem.Processed = 0
		AND MajescoItem.TransactionType = 'RETURNED_PAYMENT'
		AND m1.TransactionType = 'PAYMENT'
		AND m1.Processed = 1
		AND MajescoItem.UniqReceipt = -1
		AND m1.UniqMajescoItem = (	SELECT MIN(UniqMajescoItem)
									FROM MajescoItem
									WHERE MajescoItemNumber = MajescoItem.MajescoItemNumber
										AND TransactionType = 'PAYMENT'
										AND ItemNumber NOT IN (	SELECT ItemNumber
																FROM MajescoItem
																WHERE MajescoItemNumber = MajescoItem.MajescoItemNumber
																	AND TransactionType = 'RETURNED_PAYMENT'));
	
	-- Endorsement existing line logic --
	------------ ** START ** ------------

	UPDATE MajescoItem 
		SET UniqPolicyMatch = m1.UniqPolicyMatch,
			UniqLineMatch = m1.UniqLineMatch,
			UniqLine = m1.UniqLine,
			UniqPolicy = m1.UniqPolicy,
			BillNumber = m1.BillNumber,
			InvoiceNumber = m1.InvoiceNumber
		FROM MajescoItem m1
		INNER JOIN MajescoItem
			ON m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
				AND m1.UniqPolicy != -1
				AND m1.UniqLine != -1
		WHERE MajescoItem.TransactionType IN ('ENDORSEMENT', 'CANCELLATION')
			AND MajescoItem.Processed = 0
			AND MajescoItem.Exception IS NULL;

	UPDATE MajescoItem
	SET UniqLine =	CASE
						WHEN (	SELECT COUNT(*)
								FROM [Policy] p
								INNER JOIN Line l
									ON p.UniqPolicy = l.UniqPolicy
								INNER JOIN CdPolicyLineType lt
									ON l.UniqCdPolicyLineType = lt.UniqCdPolicyLineType
								WHERE p.PolicyNumber = MajescoItem.CarnetNumber
									AND p.EffectiveDate = MajescoItem.EffectiveDate
									AND p.ExpirationDate = MajescoItem.ExpirationDate
									AND lt.CdPolicyLineTypeCode = MajescoItem.LineType) = 0 THEN -1
						ELSE (	SELECT TOP 1 l.UniqLine
								FROM [Policy] p
								INNER JOIN Line l
									ON p.UniqPolicy = l.UniqPolicy
								INNER JOIN CdPolicyLineType lt
									ON l.UniqCdPolicyLineType = lt.UniqCdPolicyLineType
								WHERE p.PolicyNumber = MajescoItem.CarnetNumber
									AND p.EffectiveDate = MajescoItem.EffectiveDate
									AND p.ExpirationDate = MajescoItem.ExpirationDate
									AND lt.CdPolicyLineTypeCode = MajescoItem.LineType)
					END,
		UniqPolicy =	CASE
							WHEN (	SELECT COUNT(*)
									FROM [Policy] p
									INNER JOIN Line l
										ON p.UniqPolicy = l.UniqPolicy
									INNER JOIN CdPolicyLineType lt
										ON l.UniqCdPolicyLineType = lt.UniqCdPolicyLineType
									WHERE p.PolicyNumber = MajescoItem.CarnetNumber
										AND p.EffectiveDate = MajescoItem.EffectiveDate
										AND p.ExpirationDate = MajescoItem.ExpirationDate
										AND lt.CdPolicyLineTypeCode = MajescoItem.LineType) = 0 THEN -1
							ELSE (	SELECT TOP 1 p.UniqPolicy
									FROM [Policy] p
									INNER JOIN Line l
										ON p.UniqPolicy = l.UniqPolicy
									INNER JOIN CdPolicyLineType lt
										ON l.UniqCdPolicyLineType = lt.UniqCdPolicyLineType
									WHERE p.PolicyNumber = MajescoItem.CarnetNumber
										AND p.EffectiveDate = MajescoItem.EffectiveDate
										AND p.ExpirationDate = MajescoItem.ExpirationDate
										AND lt.CdPolicyLineTypeCode = MajescoItem.LineType)
						END
	WHERE TransactionType IN ('ENDORSEMENT', 'CANCELLATION')
		AND UniqPolicy = -1
		AND UniqLine = -1
		AND Processed = 0
		AND Exception IS NULL;

	--UPDATE MajescoItem
	--SET Exception = 'Existing policy/line could not be located.'
	--WHERE TransactionType IN ('ENDORSEMENT', 'CANCELLATION')
	--	AND UniqPolicy = -1
	--	AND UniqLine = -1
	--	AND Processed = 0
	--	AND Exception IS NULL;
	------------- ** END ** -------------

	-- Payment existing line logic --
	---------- ** START ** ----------

	UPDATE MajescoItem
	SET UniqPolicy = m1.UniqPolicy,
		UniqLine = m1.UniqLine
	FROM MajescoItem m1
	INNER JOIN MajescoItem
		ON m1.TransactionType IN ('ENDORSEMENT','NEW')
			AND m1.MajescoItemNumber = MajescoItem.MajescoItemNumber
			AND MajescoItem.Processed = 0
			AND MajescoItem.UniqReceipt = -1
			AND MajescoItem.TransactionType IN ('PAYMENT','PAYMENT_TRANSFER_INTERNAL', 'PAYMENT_TRANSFER_EXTERNAL','PAYMENT_TRASNFER_INTERNAL', 'PAYMENT_TRASNFER_EXTERNAL');

	------------- ** END ** -------------

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;