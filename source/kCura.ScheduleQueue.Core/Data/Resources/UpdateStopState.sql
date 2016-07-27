BEGIN TRANSACTION UpdateStopState

IF EXISTS (SELECT * FROM [eddsdbo].[{0}] WHERE [JobID] IN ({1}) AND [StopState] = 2 AND @State = 1)
BEGIN
	RAISERROR('ERROR : Invalid operation. Attempted to stop an unstoppable job.', 18, 1)
END

IF EXISTS (SELECT * FROM [eddsdbo].[{0}] WHERE [JobID] IN ({1}) AND [StopState] = 1 AND @State = 2)
BEGIN
	RAISERROR('ERROR : Invalid operation. Attempted to mark the stopping job as an unstoppable job.', 18, 1)
END

UPDATE	
	[eddsdbo].[{0}] 
SET 
	[StopState] = @State
WHERE 
	[JobID] IN ({1})

COMMIT TRANSACTION UpdateStopState;  