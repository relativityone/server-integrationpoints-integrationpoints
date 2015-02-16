SELECT
		ot.DescriptorArtifactTypeID
		,ot.Name
  FROM [EddsDBO].ObjectType ot
  WHERE DescriptorArtifactTypeID in 
  (select atg.ArtifactTypeID
  FRom [EDDSDBO].[GroupUser] gu
  join [EDDSDBO].[AccessControlListPermission]  acl on gu.GroupArtifactID = acl.GroupID
  join [EDDSDBO].[Permission] p on p.PermissionID = acl.PermissionID
  join [EDDSDBO].[ArtifactTypeGrouping] atg on atg.ArtifactGroupingID = p.ArtifactGrouping
  where UserArtifactID = @userID AND p.[Type] = 6 
)
AND IsSystem = 0
order by ot.Name