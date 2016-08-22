BEGIN TRANSACTION

DECLARE @result int

SELECT TOP 1 @result = [StopState]
	 FROM eddsdbo.[{0}] WITH(TABLOCK, HOLDLOCK)
	 WHERE [JobId] IN ({1})
	 AND
	   ([StopState] = 2 AND @state = 1 OR
	    [StopState] = 1 AND @state = 2)


IF @result = 2
  RAISERROR('ERROR : Invalid operation. Attempted to stop an unstoppable job.', 18, 1)

IF @result = 1
  RAISERROR('ERROR : Invalid operation. Attempted to mark the stopping job as an unstoppable job.', 18, 1) 

UPDATE
  [eddsdbo].[{0}]
SET
  [StopState] = @state
WHERE
  [JobId] IN ({1})

COMMIT TRANSACTION 