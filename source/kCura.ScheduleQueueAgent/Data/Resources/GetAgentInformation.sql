SELECT TOP 1 
			a.[ArtifactID] AS AgentID,
			at.[ArtifactID] AS AgentTypeID,
      at.[Name],
      at.[Fullnamespace],
      at.[Guid]
FROM 
			[eddsdbo].[AgentType]at WITH(NOLOCK)
JOIN
			[eddsdbo].[Agent]a WITH(NOLOCK) ON at.ArtifactID=a.AgentTypeArtifactID
WHERE
			(NOT @AgentID IS NULL AND a.[ArtifactID] = @AgentID)
		OR
			(NOT @AgentGuid IS NULL AND at.[Guid] = @AgentGuid)
