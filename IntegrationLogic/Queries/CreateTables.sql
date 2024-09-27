BEGIN TRY

	CREATE TABLE [dbo].[MajescoHeader](
		[UniqMajescoHeader] [int] IDENTITY(10000,1) NOT NULL,
		[RecordType] [varchar](max) NULL,
		[FileLoadNumber] [int] NULL,
		[NumberOfRecords] [int] NULL,
		[CountOfPolicies] [int] NULL,
		[CreditDebit] [varchar](max) NULL,
		[TotalTransactionAmount] [numeric](19, 4) NULL,
		[DateGenerated] [datetime] NULL,
		[Validated] [bit] NULL,
		[Processed] [bit] NULL,
		[Exception] [varchar](max) NULL,
		[FilePath] [varchar](max) NULL,
	 CONSTRAINT [PK_MajescoHeader] PRIMARY KEY CLUSTERED 
	(
		[UniqMajescoHeader] ASC
	)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

	CREATE TABLE [dbo].[MajescoItem](
	[UniqMajescoItem] [int] IDENTITY(10000,1) NOT NULL,
	[UniqMajescoHeader] [int] NOT NULL,
	[RecordType] [varchar](max) NULL,
	[EntityCode] [varchar](max) NULL,
	[CarnetNumber] [varchar](max) NULL,
	[FormNo] [varchar](max) NULL,
	[EffectiveDate] [datetime] NULL,
	[ExpirationDate] [datetime] NULL,
	[MajescoItemNumber] [int] NULL,
	[SystemActivityNumber] [varchar](max) NULL,
	[TransactionType] [varchar](max) NULL,
	[ReceivableCode] [varchar](max) NULL,
	[Amount] [numeric](19, 4) NULL,
	[DeferredAmount] [numeric](19, 4) NULL,
	[CreditDebit] [varchar](max) NULL,
	[CarnetHolder] [varchar](max) NULL,
	[BankCode] [varchar](max) NULL,
	[AccountingYearMonth] [varchar](max) NULL,
	[DepositDate] [datetime] NULL,
	[LocationId] [varchar](max) NULL,
	[UniqClient] [int] NULL,
	[UniqPolicyMatch] [int] NULL,
	[UniqLineMatch] [int] NULL,
	[UniqTransHead] [int] NULL,
	[UniqTransHeadDeferred] [int] NULL,
	[UniqPolicy] [int] NULL,
	[UniqLine] [int] NULL,
	[UniqActivity] [int] NULL,
	[UniqReceipt] [int] NULL,
	[UniqDisbursement] [int] NULL,
	[ItemNumber] [int] NULL,
	[BillNumber] [int] NULL,
	[InvoiceNumber] [int] NULL,
	[ReferNumber] [int] NULL,
	[BankAccount] [int] NULL,
	[GlAccount] [int] NULL,
	[ApplyToItemNumber] [int] NULL,
	[ApplyToTransactionId] [int] NULL,
	[ApplyToDeferredTransactionId] [int] NULL,
	[ApplyToTransactionCode] [varchar](max) NULL,
	[ApplyToEffectiveDate] [datetime] NULL,
	[LineType] [varchar](max) NULL,
	[TransactionCode] [varchar](max) NULL,
	[Description] [varchar](max) NULL,
	[ActivityCode] [varchar](max) NULL,
	[Processed] [bit] NOT NULL,
	[Exception] [varchar](max) NULL,
	[ReceivableItemSeqNumber] [int] NULL,
	[UnderwritingCompanyCode] [varchar](max) NULL,
	[UniqCompany] [int] NULL,
	[CompanyLookupCode] [varchar](max) NULL,
	[ProcessType] [varchar](max) NULL,
	[PaymentSequence] [int] NULL,
	[PaymentItemSequence] [int] NULL,
	[ProcessedDate] [datetime] NULL
 CONSTRAINT [PK_MajescoItem] PRIMARY KEY CLUSTERED 
(
	[UniqMajescoItem] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;