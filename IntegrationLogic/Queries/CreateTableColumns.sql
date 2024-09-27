BEGIN TRY

	IF NOT EXISTS (	SELECT * 
					FROM   SYS.COLUMNS 
					WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[MajescoItem]') 
							AND NAME = 'FormNo')
							ALTER TABLE MajescoItem
							ADD FormNo VARCHAR(MAX) NULL;

	IF NOT EXISTS (	SELECT * 
					FROM   SYS.COLUMNS 
					WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[MajescoItem]') 
							AND NAME = 'ReceivableItemSeqNumber')
							ALTER TABLE MajescoItem
							ADD ReceivableItemSeqNumber INT NULL;

	IF NOT EXISTS (	SELECT * 
					FROM   SYS.COLUMNS 
					WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[MajescoItem]') 
							AND NAME = 'UnderwritingCompanyCode')
							ALTER TABLE MajescoItem
							ADD UnderwritingCompanyCode VARCHAR(MAX) NULL;

	IF NOT EXISTS (	SELECT * 
					FROM   SYS.COLUMNS 
					WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[MajescoItem]') 
							AND NAME = 'UniqCompany')
							ALTER TABLE MajescoItem
							ADD UniqCompany INT NULL;

	IF NOT EXISTS (	SELECT * 
					FROM   SYS.COLUMNS 
					WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[MajescoItem]') 
							AND NAME = 'CompanyLookupCode')
							ALTER TABLE MajescoItem
							ADD CompanyLookupCode VARCHAR(MAX) NULL;

	IF NOT EXISTS (	SELECT * 
					FROM   SYS.COLUMNS 
					WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[MajescoItem]') 
							AND NAME = 'ProcessType')
							ALTER TABLE MajescoItem
							ADD ProcessType VARCHAR(MAX) NULL;

	IF NOT EXISTS (	SELECT * 
					FROM   SYS.COLUMNS 
					WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[MajescoItem]') 
							AND NAME = 'PaymentSequence')
							ALTER TABLE MajescoItem
							ADD PaymentSequence INT NULL;

	IF NOT EXISTS (	SELECT * 
					FROM   SYS.COLUMNS 
					WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[MajescoItem]') 
							AND NAME = 'PaymentItemSequence')
							ALTER TABLE MajescoItem
							ADD PaymentItemSequence VARCHAR(MAX) NULL;

	IF NOT EXISTS (	SELECT * 
					FROM   SYS.COLUMNS 
					WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[MajescoItem]') 
							AND NAME = 'CustomerEntityCode')
							ALTER TABLE MajescoItem
							ADD CustomerEntityCode VARCHAR(MAX) NULL;

	IF NOT EXISTS (	SELECT * 
					FROM   SYS.COLUMNS 
					WHERE  OBJECT_ID = OBJECT_ID(N'[dbo].[MajescoItem]') 
							AND NAME = 'ProcessedDate')
							ALTER TABLE MajescoItem
							ADD ProcessedDate DATETIME NULL;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;