SELECT
ot.DescriptorArtifactTypeID
,ot.Name
FROM [EDDSDBO].ObjectType ot
WHERE DescriptorArtifactTypeID in
(select atg.ArtifactTypeID
From [EDDSDBO].[GroupUser] gu
join [EDDSDBO].[AccessControlListPermission]  acl on gu.GroupArtifactID = acl.GroupID
join [EDDSDBO].[Permission] p on p.PermissionID = acl.PermissionID
join [EDDSDBO].[ArtifactTypeGrouping] atg on atg.ArtifactGroupingID = p.ArtifactGrouping
where UserArtifactID = @userID AND p.[Type] = 6
)
AND (DescriptorArtifactTypeID > 1000000 OR DescriptorArtifactTypeID = 10)
order by ot.Name
