BEGIN TRY

	-- Payment post processing messaging
	UPDATE MajescoItem
	SET Exception = 'Error creating receipt: Existing transaction with Item Number ' + CONVERT(VARCHAR, (SELECT ItemNumber FROM TransHead WHERE UniqTransHead = ApplyToTransactionId)) + ' has a balance of $0.00.'
	WHERE Processed = 0
		AND TransactionType IN ('PAYMENT','PAYMENT_TRANSFER_INTERNAL','PAYMENT_TRASNFER_INTERNAL','PAYMENT_TRANSFER_EXTERNAL','PAYMENT_TRASNFER_EXTERNAL')
		AND Exception LIKE '%Debit Transaction ID not found%'
		AND ApplyToTransactionId NOT IN (-1, 0)
		AND 1 = CASE
					WHEN (	SELECT ItemBalanceCalc
							FROM TransDetail
							WHERE UniqTransHead = ApplyToTransactionId
								AND TransDetailNumber = (	SELECT MAX(TransDetailNumber)
															FROM TransDetail
															WHERE UniqTransHead = ApplyToTransactionId)) = 0 THEN 1
					ELSE 0
				END;
				
	-- Payment Adjustment post processing messaging
	UPDATE MajescoItem
	SET Exception = 'Payment Adjustment cannot be processed. A payment has not been made to Majesco Item Number ' + CONVERT(VARCHAR, MajescoItemNumber) + '.'
	WHERE UniqReceipt = -1
		AND TransactionType = 'PAYMENT_ADJUSTMENT'
		AND Processed = 0;

	-- Update Reference Number for processed payments
	UPDATE MajescoItem
	SET ReferNumber = (	SELECT ReferNumber
						FROM Receipt
						WHERE UniqReceipt = MajescoItem.UniqReceipt)
	WHERE UniqReceipt != -1
		AND Processed = 1
		AND ReferNumber IN (-1,0)
		AND TransactionType LIKE '%PAYMENT%';

	-- Update Transction ID
	UPDATE MajescoItem
	SET UniqTransHead = -1
	WHERE Processed = 1
		AND UniqTransHead = 0;

	-- Update Bill Number
	UPDATE MajescoItem
	SET BillNumber = -1
	WHERE Processed = 1
		AND BillNumber = 0;

	-- Update Majesco Item with receipt details
	UPDATE MajescoItem
	SET ItemNumber = (	SELECT th.ItemNumber
						FROM TransHead th
						INNER JOIN TransDetail td
							ON th.UniqTransHead = td.UniqTransHead
						WHERE td.ItemAmount != -1
							AND td.UniqReceipt = MajescoItem.UniqReceipt
							AND td.TransDetailNumber = 1),
		UniqTransHead = (	SELECT th.ItemNumber
							FROM TransHead th
							INNER JOIN TransDetail td
								ON th.UniqTransHead = td.UniqTransHead
							WHERE td.ItemAmount != -1
								AND td.UniqReceipt = MajescoItem.UniqReceipt
								AND td.TransDetailNumber = 1),
		BillNumber = (	SELECT th.ItemNumber
						FROM TransHead th
						INNER JOIN TransDetail td
							ON th.UniqTransHead = td.UniqTransHead
						WHERE td.ItemAmount != -1
							AND td.UniqReceipt = MajescoItem.UniqReceipt
							AND td.TransDetailNumber = 1)
	WHERE UniqReceipt != -1
		AND ItemNumber = -1
		AND BillNumber = -1
		AND InvoiceNumber = -1
		AND Processed = 1;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;