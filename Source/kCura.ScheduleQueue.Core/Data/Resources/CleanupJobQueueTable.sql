--If an agent was deleted while a job was running and not completed, we remove the agent lock
--from the job so that another agent can pick it up.

DECLARE @agentArtifactIds TABLE(ArtifactId int)

INSERT INTO @agentArtifactIds
SELECT A.[ArtifactID]
FROM [eddsdbo].[Agent] as A WITH(NOLOCK)
INNER JOIN [eddsdbo].[AgentType] as AT WITH(NOLOCK)
ON A.[AgentTypeArtifactID] = AT.[ArtifactID]
WHERE AT.[Guid] = @agentGuid

UPDATE [eddsdbo].[{0}] WITH(UPDLOCK, READPAST, ROWLOCK)
SET [LockedByAgentID] = NULL
FROM [eddsdbo].[{0}] as SAQ WITH(UPDLOCK, READPAST, ROWLOCK)
WHERE SAQ.[LockedByAgentID] IS NOT NULL AND SAQ.[LockedByAgentID] NOT IN (
	SELECT [ArtifactId]
	FROM @agentArtifactIds
)
