BEGIN TRY

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_Client_LookupCode') = 0	CREATE NONCLUSTERED INDEX Idx_Client_LookupCode
													ON dbo.Client (UniqEntity, LookupCode);
	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_Line_SiteID') = 0	CREATE NONCLUSTERED INDEX Idx_Line_SiteID
												ON dbo.Line (UniqLine, UniqCdPolicyLineType, SiteID);
	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_Line_CdPolicyLineType') = 0	CREATE NONCLUSTERED INDEX Idx_Line_CdPolicyLineType
												ON dbo.Line (UniqCdPolicyLineType) INCLUDE (UniqPolicy, SiteID);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_TransDetail_TransDetailNumber') = 0	CREATE NONCLUSTERED INDEX Idx_TransDetail_TransDetailNumber
																ON dbo.TransDetail (TransDetailNumber, UniqReceipt, ItemAmount);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_PolicyLine_SiteID') = 0	CREATE NONCLUSTERED INDEX Idx_PolicyLine_SiteID
													ON dbo.Line (UniqPolicy, UniqCdPolicyLineType, SiteID);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_LineAgencyDefined_CategoryCode') = 0	CREATE NONCLUSTERED INDEX Idx_LineAgencyDefined_CategoryCode
																	ON dbo.LineAgencyDefined (UniqLine, CategoryCode);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoHeader') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoHeader
												ON dbo.MajescoHeader (UniqMajescoHeader);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_UniqMajescoHeader') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_UniqMajescoHeader
																ON dbo.MajescoItem (UniqMajescoItem, UniqMajescoHeader);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_UniqClient') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_UniqClient
															ON dbo.MajescoItem (UniqMajescoItem, UniqClient);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_Header') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_Header
														ON dbo.MajescoItem (UniqMajescoHeader, Processed, UniqClient) INCLUDE (LocationId, LineType, Exception);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_Policy') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_Policy
														ON dbo.MajescoItem (UniqPolicy, UniqLine) INCLUDE (MajescoItemNumber, UniqLineMatch, BillNumber, InvoiceNumber);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_MajescoItemNumber') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_MajescoItemNumber
																ON dbo.MajescoItem (MajescoItemNumber) INCLUDE (UniqPolicyMatch, UniqLineMatch, UniqPolicy, UniqLine, BillNumber, InvoiceNumber);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_Processed') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_Processed
														ON dbo.MajescoItem (Processed, UniqReceipt) INCLUDE (TransactionType);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_ItemProcessed') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_ItemProcessed
														ON dbo.MajescoItem (Processed) INCLUDE (MajescoItemNumber, TransactionType, UniqPolicy, UniqLine, UniqReceipt, BillNumber);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_ItemNumber') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_ItemNumber
														ON dbo.MajescoItem (ItemNumber)

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_UniqTransHead') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_UniqTransHead
															ON dbo.MajescoItem (UniqTransHead)

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_EntityCode') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_EntityCode
															ON dbo.MajescoItem (UniqMajescoItem) INCLUDE (EntityCode, CustomerEntityCode);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_Disbursement') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_Disbursement
															ON dbo.MajescoItem (Processed, UniqDisbursement);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_TransItemNumber') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_TransItemNumber
																ON dbo.MajescoItem (ItemNumber, UniqTransHead);

	IF (SELECT COUNT(*)
		FROM SYS.INDEXES
		WHERE [Name] = 'Idx_MajescoItem_BillNumber') = 0	CREATE NONCLUSTERED INDEX Idx_MajescoItem_BillNumber
															ON dbo.MajescoItem (BillNumber, Processed);

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;