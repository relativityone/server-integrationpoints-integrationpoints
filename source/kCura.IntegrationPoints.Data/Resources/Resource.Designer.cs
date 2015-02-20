﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace kCura.IntegrationPoints.Data.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("kCura.IntegrationPoints.Data.Resources.Resource", typeof(Resource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 	select gu.UserArtifactID 
        ///  From [EDDSDBO].[GroupUser] gu
        ///  join [EDDSDBO].[AccessControlListPermission]  acl on gu.GroupArtifactID = acl.GroupID
        ///  join [EDDSDBO].[Permission] p on p.PermissionID = acl.PermissionID
        ///  where UserArtifactID = @userID AND p.[PermissionID] = 158
        ///.
        /// </summary>
        internal static string CheckImportPermission {
            get {
                return ResourceManager.GetString("CheckImportPermission", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SET ANSI_NULLS ON
        ///SET QUOTED_IDENTIFIER ON
        ///
        ///--Do cleanup first - delete old tables (over 24 hours old)
        ///DECLARE @table varchar(255) 
        ///DECLARE @dropCommand varchar(300) 
        ///
        ///DECLARE tableCursor CURSOR FOR 
        ///		SELECT QUOTENAME(&apos;EDDSResource&apos;)+&apos;.&apos;+QUOTENAME(s.name)+&apos;.&apos;+QUOTENAME(t.name) 
        ///		FROM [EDDSResource].[sys].[tables] AS t 
        ///		INNER JOIN [EDDSResource].[sys].[schemas] AS s 
        ///		ON t.[schema_id] = s.[schema_id] 
        ///		WHERE DATEDIFF(HOUR,t.create_date,GETUTCDATE())&gt;24 
        ///		AND t.name LIKE &apos;RIP_CustodianMana [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateCustodianManagerResourceTable {
            get {
                return ResourceManager.GetString("CreateCustodianManagerResourceTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///					RF.[ArtifactID]
        ///					,RF.[Name]
        ///					,RFD.[FileData]
        ///FROM 
        ///					[EDDSDBO].[ResourceFile] RF WITH(NOLOCK)
        ///	JOIN  
        ///					[EDDSDBO].[ResourceFileData] RFD WITH(NOLOCK)
        ///		ON		
        ///					RF.[ArtifactID] = RFD.[ArtifactID]
        ///WHERE 
        ///					RF.[ApplicationGuid]=@ApplicationGuid
        ///	.
        /// </summary>
        internal static string GetApplicationBinaries {
            get {
                return ResourceManager.GetString("GetApplicationBinaries", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT		[ArtifactGuid]
        ///FROM			[EDDSDBO].[ArtifactGuid] AG WITH(NOLOCK)
        ///WHERE 		AG.[ArtifactID] = @ApplicationID.
        /// </summary>
        internal static string GetApplicationGuid {
            get {
                return ResourceManager.GetString("GetApplicationGuid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	ArtifactId
        ///FROM
        ///	[EDDSDBO].[ArtifactGuid] WITH (NOLOCK)
        ///WHERE [ArtifactGuid]= @ArtifactGuid.
        /// </summary>
        internal static string GetArtifactIDByGuid {
            get {
                return ResourceManager.GetString("GetArtifactIDByGuid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to --bypass duplicate records
        ///UPDATE	[EDDSResource].[eddsdbo].[{0}]
        ///SET
        ///				[LockedByJobID]	= -1
        ///FROM 
        ///				[EDDSResource].[eddsdbo].[{0}] t1 
        ///JOIN
        ///				(
        ///					SELECT * FROM [EDDSResource].[eddsdbo].[{0}] WHERE NOT [LockedByJobID] IS NULL
        ///				) t2
        ///	ON		t1.[CustodianID] = t2.[CustodianID] AND t1.[ManagerID] = t2.[ManagerID] 
        ///WHERE
        ///				t1.[LockedByJobID] IS NULL
        ///				
        ///
        ///--get next batch
        ///UPDATE	[EDDSResource].[eddsdbo].[{0}]
        ///SET
        ///				[LockedByJobID]	= @JobID
        ///OUTPUT 
        ///				INSERTED.[CustodianID],
        ///			 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetJobCustodianManagerLinks {
            get {
                return ResourceManager.GetString("GetJobCustodianManagerLinks", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///		ot.DescriptorArtifactTypeID
        ///		,ot.Name
        ///  FROM [EDDSDBO].ObjectType ot
        ///  WHERE DescriptorArtifactTypeID in 
        ///  (select atg.ArtifactTypeID
        ///  From [EDDSDBO].[GroupUser] gu
        ///  join [EDDSDBO].[AccessControlListPermission]  acl on gu.GroupArtifactID = acl.GroupID
        ///  join [EDDSDBO].[Permission] p on p.PermissionID = acl.PermissionID
        ///  join [EDDSDBO].[ArtifactTypeGrouping] atg on atg.ArtifactGroupingID = p.ArtifactGrouping
        ///  where UserArtifactID = @userID AND p.[Type] = 6 
        ///)
        ///AND (DescriptorArtifac [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetObjectTypes {
            get {
                return ResourceManager.GetString("GetObjectTypes", resourceCulture);
            }
        }
    }
}
