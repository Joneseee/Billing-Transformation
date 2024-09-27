BEGIN TRY

	DECLARE @RecordType AS VARCHAR(MAX) = @@recordType;
	DECLARE @FileLoadNumber AS INT = @@fileLoadNumber;
	DECLARE @NumberOfRecords AS INT = @@numberOfRecords;
	DECLARE @CountOfPolicies AS INT = @@countOfPolicies;
	DECLARE @CreditDebit AS VARCHAR(MAX) = @@creditDebit;
	DECLARE @TotalTransactionsAmount AS NUMERIC(19,4) = @@totalTransactionsAmount;
	DECLARE @DateGenerated AS DATETIME = @@dateGenerated;
	DECLARE @FilePath AS VARCHAR(MAX) = @@filePath;

	INSERT INTO MajescoHeader (	RecordType, 
								FileLoadNumber, 
								NumberOfRecords,
								CountOfPolicies,
								CreditDebit,
								TotalTransactionAmount,
								DateGenerated,
								Validated,
								Processed,
								FilePath)
	VALUES (@RecordType,
			@FileLoadNumber,
			@NumberOfRecords,
			@CountOfPolicies,
			@CreditDebit,
			@TotalTransactionsAmount,
			@DateGenerated,
			0,
			0,
			@filePath);

	SELECT SCOPE_IDENTITY();

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;