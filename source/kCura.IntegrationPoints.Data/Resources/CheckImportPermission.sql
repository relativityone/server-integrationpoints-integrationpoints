select gu.UserArtifactID 
  From [EDDSDBO].[GroupUser] gu
  join [EDDSDBO].[AccessControlListPermission]  acl on gu.GroupArtifactID = acl.GroupID
  join [EDDSDBO].[Permission] p on p.PermissionID = acl.PermissionID
  where UserArtifactID = @userID AND p.[PermissionID] = 158
