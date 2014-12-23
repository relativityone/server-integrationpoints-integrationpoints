UPDATE	
					[eddsdbo].[{0}] 
SET 
					[NextRunTime] = @NextRunTime, 
					[LockedByAgentID] = NULL 
WHERE 
					[JobID] = @JobID