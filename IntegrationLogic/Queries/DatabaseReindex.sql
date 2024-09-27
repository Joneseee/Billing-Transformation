BEGIN TRY

	dbcc dbreindex([Client])
	dbcc dbreindex([Policy])
	dbcc dbreindex([Line])
	dbcc dbreindex([LineAgencyDefined])
	dbcc dbreindex([TransHead])
	dbcc dbreindex([TransDetail])
	dbcc dbreindex([MajescoHeader])
	dbcc dbreindex([MajescoItem])

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;