SELECT TOP 1 
			a.[ArtifactID] AS AgentID,
			at.[ArtifactID] AS AgentTypeID,
      [Name],
      [Fullnamespace],
      [Guid]
FROM 
			[eddsdbo].[AgentType]at WITH(NOLOCK)
JOIN
			[eddsdbo].[Agent]a WITH(NOLOCK) ON at.ArtifactID=a.AgentTypeArtifactID
WHERE
			a.ArtifactID = @AgentID