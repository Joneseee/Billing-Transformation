BEGIN TRY

	-- Creates a virtual payment for REFUNDs with no payments.
	IF OBJECT_ID('tempdb..#tmpVirtualPayment') IS NOT NULL
		DROP TABLE #tmpVirtualPayment;

	SELECT	MinUniqMajescoItem = -1,
			*
	INTO #tmpVirtualPayment
	FROM MajescoItem
	WHERE Processed = 1
		AND UniqMajescoItem IN (	SELECT UniqMajescoItem
									FROM MajescoItem
									WHERE MajescoItemNumber IN (	SELECT MajescoItemNumber
																	FROM MajescoItem
																	WHERE Processed = 0
																	AND TransactionType = 'REFUND'
																	AND MajescoItemNumber NOT IN (	SELECT MajescoItemNumber
																									FROM MajescoItem
																									WHERE TransactionType IN ('PAYMENT','VIRTUAL_PAYMENT'))));

	UPDATE #tmpVirtualPayment
	SET MinUniqMajescoItem = (	SELECT MIN(UniqMajescoItem)
								FROM MajescoItem
								WHERE TransactionType = 'ENDORSEMENT'
									AND MajescoItemNumber = #tmpVirtualPayment.MajescoItemNumber);

	DELETE FROM #tmpVirtualPayment
	WHERE MinUniqMajescoItem != UniqMajescoItem;

	INSERT INTO MajescoItem (	UniqMajescoHeader,
								RecordType,
								EffectiveDate,
								ExpirationDate,
								EntityCode,
								CarnetNumber,
								FormNo,
								MajescoItemNumber,
								SystemActivityNumber,
								TransactionType,
								ReceivableCode,
								Amount,
								DeferredAmount,
								CreditDebit,
								CarnetHolder,
								BankCode,
								AccountingYearMonth,
								DepositDate,
								LocationId,
								UniqClient,
								UniqPolicyMatch,
								UniqLineMatch,
								UniqTransHead,
								UniqTransHeadDeferred,
								UniqPolicy,
								UniqLine,
								UniqActivity,
								UniqReceipt,
								UniqDisbursement,
								ItemNumber,
								BillNumber,
								InvoiceNumber,
								ReferNumber,
								BankAccount,
								GLAccount,
								ApplyToItemNumber,
								ApplyToTransactionId,
								ApplyToDeferredTransactionId,
								ApplyToTransactionCode,
								ApplyToEffectiveDate,
								LineType,
								TransactionCode,
								[Description],
								ActivityCode,
								Processed,
								Exception,
								ReceivableItemSeqNumber,
								UnderwritingCompanyCode,
								UniqCompany,
								CompanyLookupCode,
								ProcessType,
								PaymentSequence,
								PaymentItemSequence,
								CustomerEntityCode)
	SELECT	UniqMajescoHeader,
			RecordType,
			EffectiveDate,
			ExpirationDate,
			EntityCode,
			CarnetNumber,
			FormNo,
			MajescoItemNumber,
			SystemActivityNumber,
			'VIRTUAL_PAYMENT',
			ReceivableCode,
			Amount,
			DeferredAmount = 0,
			CreditDebit =	CASE
								WHEN CreditDebit = 'CR' THEN 'DB'
								ELSE 'CR'
							END,
			CarnetHolder,
			BankCode,
			AccountingYearMonth,
			DepositDate,
			LocationId,
			UniqClient,
			UniqPolicyMatch,
			UniqLineMatch,
			-1,
			-1,
			UniqPolicy,
			UniqLine,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			BankAccount,
			GLAccount,
			ItemNumber,
			UniqTransHead,
			-1,
			TransactionCode,
			ApplyToEffectiveDate,
			LineType,
			TransactionCode,
			[Description],
			'MBA3',
			0,
			NULL,
			ReceivableItemSeqNumber,
			UnderwritingCompanyCode,
			UniqCompany,
			CompanyLookupCode,
			ProcessType,
			PaymentSequence,
			PaymentItemSequence,
			CustomerEntityCode
	FROM MajescoItem
	WHERE UniqMajescoItem IN (	SELECT UniqMajescoItem
								FROM #tmpVirtualPayment);

	DROP TABLE #tmpVirtualPayment;

END TRY

BEGIN CATCH

	IF OBJECT_ID('tempdb..#tmpVirtualPayment') IS NOT NULL
			DROP TABLE #tmpVirtualPayment;

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;