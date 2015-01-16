SELECT		[ArtifactGuid]
FROM			[EDDSDBO].[ApplicationGuid] AG WITH(NOLOCK)
WHERE 		AG.[ApplicationID] = @ApplicationID