DECLARE @tasksFinished BIT SELECT @tasksFinished = 1
IF EXISTS(SELECT * FROM [eddsdbo].[{0}] WHERE [RootJobID] = @RootJobID AND [TaskType] = 'SyncWorker') BEGIN
	SET @tasksFinished = 0
END
SELECT @tasksFinished