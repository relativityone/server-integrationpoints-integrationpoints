
BEGIN TRANSACTION UpdateStopState


IF NOT EXISTS (SELECT * FROM [eddsdbo].[{0}] WHERE [JobID] = @JobID)
BEGIN
	RAISERROR('ERROR : Job does not exist', 18, 1)
END

IF EXISTS (SELECT * FROM [eddsdbo].[{0}] WHERE [JobID] = @JobID AND [StopState] = 2 AND @State = 1)
BEGIN
	RAISERROR('ERROR : Unable to stop an unstoppable job.', 18, 1)
END

UPDATE	
					[eddsdbo].[{0}] 
SET 
					[StopState] = @State
WHERE 
					[JobID] = @JobID

COMMIT TRANSACTION UpdateStopState;  