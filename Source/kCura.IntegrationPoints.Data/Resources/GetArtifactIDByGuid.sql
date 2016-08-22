SELECT
	ArtifactId
FROM
	[EDDSDBO].[ArtifactGuid] WITH (NOLOCK)
WHERE [ArtifactGuid]= @ArtifactGuid