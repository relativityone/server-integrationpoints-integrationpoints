--If an agent was deleted while a job was running and not completed, we remove the agent lock
--from the job so that another agent can pick it up.

UPDATE [eddsdbo].[{0}] WITH(UPDLOCK, ROWLOCK, READPAST)
SET [LockedByAgentID] = NULL
WHERE JobID IN (
	SELECT SAQ.[JobID]
	FROM [eddsdbo].[{0}] as SAQ WITH(UPDLOCK, ROWLOCK, READPAST)
	LEFT JOIN [eddsdbo].[Agent] as A WITH(NOLOCK)
	ON SAQ.[AgentTypeID] = A.[AgentTypeArtifactID]
	WHERE SAQ.[LockedByAgentID] NOT IN (A.ArtifactID)
)