BEGIN TRY

	DECLARE @UniqMajescoHeader AS INT = @@uniqMajescoHeader;
	DECLARE @RecordType AS VARCHAR(MAX) = @@recordType;
	DECLARE @EntityCode AS VARCHAR(MAX) = @@entityCode;
	DECLARE @CarnetNumber AS VARCHAR(MAX) = @@carnetNumber;
	DECLARE @EffectiveDate AS DATETIME = @@effectiveDate;
	DECLARE @ExpirationDate AS DATETIME = @@expirationDate;
	DECLARE @MajescoItemNumber AS INT = @@majescoItemNumber;
	DECLARE @SystemActivityNumber AS VARCHAR(MAX) = @@systemActivityNumber;
	DECLARE @TransactionType AS VARCHAR(MAX) = @@transactionType;
	DECLARE @ReceivableCode AS VARCHAR(MAX) = @@receivableCode;
	DECLARE @Amount AS NUMERIC(19,4) = @@amount;
	DECLARE @DeferredAmount AS NUMERIC(19,4) = @@deferredAmount;
	DECLARE @CreditDebit AS VARCHAR(MAX) = @@creditDebit;
	DECLARE @CarnetHolder AS VARCHAR(MAX) = @@carnetHolder;
	DECLARE @BankCode AS VARCHAR(MAX) = @@bankCode;
	DECLARE @BankAccount AS INT = @@bankAccount;
	DECLARE @GlAccount AS INT = @@glAccount;
	DECLARE @ReferNumber AS INT = @@referNumber;
	DECLARE @AccountingYearMonth AS VARCHAR(MAX) = @@accountingYearMonth;
	DECLARE @DepositDate AS DATETIME = @@depositDate;
	DECLARE @LocationId AS VARCHAR(MAX) = @@locationId;
	DECLARE @LineType AS VARCHAR(MAX) = @@lineType;
	DECLARE @TransactionCode AS VARCHAR(MAX) = @@transactionCode;
	DECLARE @Description AS VARCHAR(MAX) = @@description;
	DECLARE @ActivityCode AS VARCHAR(MAX) = @@activityCode;
	DECLARE @ReceivableItemSequence AS VARCHAR(MAX) = @@receivableItemSequence;
	DECLARE @UnderwritingCompanyCode AS VARCHAR(MAX) = @@underwritingCompanyCode;
	DECLARE @PaymentSequence AS INT= @@paymentSequence;
	DECLARE @PaymentItemSequence AS INT = @@paymentItemSequence;
	DECLARE @ProcessType AS VARCHAR(MAX) = @@processType;
	DECLARE @FormNo AS VARCHAR(MAX) = @@formNo;
	DECLARE @CustomerEntityCode AS VARCHAR(MAX) = @@customerEntityCode;

	INSERT INTO MajescoItem (	UniqMajescoHeader,
								RecordType, 
								EntityCode, 
								CarnetNumber, 
								EffectiveDate,
								ExpirationDate,
								MajescoItemNumber,
								SystemActivityNumber,
								TransactionType,
								ReceivableCode,
								Amount,
								DeferredAmount,
								CreditDebit,
								CarnetHolder,
								BankCode,
								BankAccount,
								GlAccount,
								ReferNumber,
								AccountingYearMonth,
								DepositDate,
								LocationId,
								LineType,
								TransactionCode,
								Description,
								ActivityCode,
								UniqClient,
								UniqPolicyMatch,
								UniqLineMatch,
								UniqTransHead,
								UniqPolicy,
								UniqLine,
								UniqActivity,
								UniqReceipt,
								UniqDisbursement,
								InvoiceNumber,
								ItemNumber,
								BillNumber,
								Processed,
								ApplyToItemNumber,
								ApplyToTransactionId,
								ApplyToTransactionCode,
								ApplyToEffectiveDate,
								ReceivableItemSeqNumber,
								UnderwritingCompanyCode,
								PaymentSequence,
								PaymentItemSequence,
								ProcessType,
								FormNo,
								CustomerEntityCode)
	VALUES (@UniqMajescoHeader,
			@RecordType,
			@EntityCode,
			@CarnetNumber,
			@EffectiveDate,
			@ExpirationDate,
			@MajescoItemNumber,
			@SystemActivityNumber,
			@TransactionType,
			@ReceivableCode,
			@Amount,
			@DeferredAmount,
			@CreditDebit,
			@CarnetHolder,
			@BankCode,
			@BankAccount,
			@GlAccount,
			@ReferNumber,
			@AccountingYearMonth,
			@DepositDate,
			@LocationId,
			@LineType,
			@TransactionCode,
			@Description,
			@ActivityCode,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			0,
			-1,
			-1,
			'',
			GETUTCDATE(),
			@ReceivableItemSequence,
			@UnderwritingCompanyCode,
			@PaymentSequence,
			@PaymentItemSequence,
			@ProcessType,
			@FormNo,
			@CustomerEntityCode);

	SELECT SCOPE_IDENTITY();

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;


