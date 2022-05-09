SELECT count(*) as Total,
(SELECT count(*) from [eddsdbo].[{0}] q
			INNER JOIN [eddsdbo].[Case] c 
			ON q.[WorkspaceID] = c.[ArtifactID]
				where [NextRunTime] <= GETUTCDATE()
				AND (q.StopState NOT IN (0,8) 
					or q.AgentTypeID != @AgentTypeID
					or c.ResourceGroupArtifactID NOT IN (@ResourceGroupArtifactIDs))) as Blocked
FROM [eddsdbo].[{0}]
WHERE [NextRunTime] <= GETUTCDATE()
