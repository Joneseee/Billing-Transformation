BEGIN TRY
	
	-- Updates Item Number where transaction exists, but Item Number remains unpopulated
	UPDATE MajescoItem
	SET ItemNumber = (	SELECT ItemNumber
						FROM TransHead
						WHERE UniqTransHead = MajescoItem.UniqTransHead)
	WHERE UniqTransHead != -1
		AND ItemNumber = -1;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;