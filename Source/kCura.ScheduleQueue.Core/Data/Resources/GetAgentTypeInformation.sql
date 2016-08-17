SELECT TOP 1 
			at.[ArtifactID] AS AgentTypeID,
      at.[Name],
      at.[Fullnamespace],
      at.[Guid]
FROM 
			[eddsdbo].[AgentType]at WITH(NOLOCK)
WHERE
			at.[Guid] = @AgentGuid
