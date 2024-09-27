BEGIN TRY
	
	-- Update Item Number, Transaction ID, and BillNumber for created receipts
	UPDATE MajescoItem
	SET ItemNumber = th.ItemNumber,
		UniqTransHead = th.UniqTransHead,
		BillNumber = th.BillNumber
	FROM MajescoItem
	INNER JOIN TransDetail td
		ON MajescoItem.UniqReceipt = td.UniqReceipt
			AND td.TransDetailNumber = 1
	INNER JOIN TransHead th
		ON th.UniqTransHead = td.UniqTransHead
	WHERE MajescoItem.UniqReceipt != -1
		AND MajescoItem.UniqTransHead = -1
		AND MajescoItem.ItemNumber = -1
		AND MajescoItem.BillNumber = -1;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;