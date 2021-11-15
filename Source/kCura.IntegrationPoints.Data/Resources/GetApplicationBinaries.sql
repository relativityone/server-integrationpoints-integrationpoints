SELECT 
					RF.[ArtifactID]
					,RF.[Name]
					,RFD.[FileData]
FROM 
					[EDDSDBO].[ResourceFile] RF WITH(NOLOCK)
	JOIN  
					[EDDSDBO].[ResourceFileData] RFD WITH(NOLOCK)
		ON		
					RF.[ArtifactID] = RFD.[ArtifactID]
WHERE 
					RF.[ApplicationGuid]=@ApplicationGuid
	AND				RF.[FileType]=0