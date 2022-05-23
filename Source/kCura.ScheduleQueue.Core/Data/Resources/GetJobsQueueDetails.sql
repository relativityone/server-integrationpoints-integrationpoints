SELECT count(*) as Total,
(SELECT count(*) from [eddsdbo].[{0}] q WITH (UPDLOCK, READPAST, ROWLOCK, INDEX([IX_{0}_LockedByAgentID_AgentTypeID_NextRunTime]))			
				where [NextRunTime] <= GETUTCDATE()
				AND (q.StopState NOT IN (0,8) 
					or q.AgentTypeID != @AgentTypeID)) as Blocked
FROM [eddsdbo].[{0}]
WHERE [NextRunTime] <= GETUTCDATE()
