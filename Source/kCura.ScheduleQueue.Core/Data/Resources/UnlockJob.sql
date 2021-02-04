UPDATE 
				[eddsdbo].[{0}]
SET
				[LockedByAgentID] = NULL
FROM 
				[eddsdbo].[{0}] WITH (UPDLOCK, ROWLOCK)
WHERE
				[JobID] = @JobID