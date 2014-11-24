UPDATE	
					[eddsdbo].[{0}] 
SET 
					[NextRunTime] = @NextRunTime, 
					[AgentID] = NULL 
WHERE 
					[JobID] = @JobID