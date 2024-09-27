BEGIN TRY
	-- Check if Payment Adjustment should be processed as a Payment
	DECLARE @MajescoItemNumber AS INT = @@majescoItemNumber;

	SELECT Result =	CASE
						WHEN (	SELECT COUNT(*)
								FROM MajescoItem
								WHERE MajescoItemNumber = @MajescoItemNumber
									AND TransactionType IN ('PAYMENT','PAYMENT_TRANSFER_INTERNAL', 'PAYMENT_TRANSFER_EXTERNAL','PAYMENT_TRASNFER_INTERNAL', 'PAYMENT_TRASNFER_EXTERNAL')) = 0 THEN	CASE
																																																		WHEN (	SELECT COUNT(*)
																																																				FROM MajescoItem
																																																				WHERE MajescoItemNumber = @MajescoItemNumber
																																																					AND TransactionType = 'ENDORSEMENT'
																																																					AND Processed = 1) > 0 THEN 1
																																																		ELSE 0
																																																	END
						ELSE 0
					END;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;