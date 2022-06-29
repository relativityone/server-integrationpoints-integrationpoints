BEGIN TRANSACTION

UPDATE
  [eddsdbo].[{0}]
SET
  [Heartbeat] = @HeartbeatTime
WHERE
  [JobId] = @JobID
SELECT @@ROWCOUNT

COMMIT TRANSACTION 