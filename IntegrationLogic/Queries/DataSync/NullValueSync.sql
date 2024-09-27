BEGIN TRY

	-- Updates Transaction ID, Bill Number, and Item Number to -1 when NULL
	UPDATE MajescoItem
	SET BillNumber = -1
	WHERE BillNumber IS NULL;

	UPDATE MajescoItem
	SET ItemNumber = -1
	WHERE ItemNumber IS NULL;

	UPDATE MajescoItem
	SET UniqTransHead = -1
	WHERE UniqTransHead IS NULL;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;