SELECT ot.ArtifactID, ot.DescriptorArtifactTypeID, ot.Name
FROM [EDDSDBO].ObjectType ot WITH(NOLOCK)
WHERE DescriptorArtifactTypeID in
(
	SELECT atg.ArtifactTypeID
	FROM [EDDSDBO].[GroupUser] gu WITH(NOLOCK)
	JOIN [EDDSDBO].[AccessControlListPermission]  acl WITH(NOLOCK) on gu.GroupArtifactID = acl.GroupID
	JOIN [EDDSDBO].[Permission] p WITH(NOLOCK) on p.PermissionID = acl.PermissionID
	JOIN [EDDSDBO].[ArtifactTypeGrouping] atg WITH(NOLOCK) on atg.ArtifactGroupingID = p.ArtifactGrouping
	WHERE p.[Type] = 1
	AND 
	UserArtifactID in 
	(
		SELECT [CaseUserID] FROM [EDDSDBO].[UserIdGenerator] WITH(NOLOCK) WHERE [CaseUserID] = @userID OR [UserArtifactID] = @userID
		UNION
		SELECT [UserArtifactID] FROM [EDDSDBO].[UserIdGenerator] WITH(NOLOCK) WHERE [CaseUserID] = @userID OR [UserArtifactID] = @userID
		UNION
		SELECT @userID
	)
)
AND (DescriptorArtifactTypeID > 1000000 OR DescriptorArtifactTypeID = 10)
ORDER BY ot.Name
