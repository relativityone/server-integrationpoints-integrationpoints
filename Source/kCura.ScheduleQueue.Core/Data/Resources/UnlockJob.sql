UPDATE 
				[eddsdbo].[{0}]
SET
				[LockedByAgentID] = NULL,
				[StopState] = @StopState
FROM 
				[eddsdbo].[{0}] WITH (UPDLOCK, ROWLOCK)
WHERE
				[JobID] = @JobID