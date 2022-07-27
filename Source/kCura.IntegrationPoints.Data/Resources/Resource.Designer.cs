﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
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
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
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
        ///   Looks up a localized string similar to IF (NOT EXISTS(SELECT 1 FROM [EDDS].[eddsdbo].[InstanceSetting] WHERE [Section] = N&apos;kCura.IntegrationPoints&apos; AND [Name] = N&apos;ReplaceWebAPIWithExportCore&apos; AND [MachineName] = &apos;&apos;))
        ///    EXEC [eddsdbo].CreateInstanceSetting @section = &apos;kCura.IntegrationPoints&apos;, @name = &apos;ReplaceWebAPIWithExportCore&apos;, @machineName = &apos;&apos;, @value = &apos;true&apos;, @description = &apos;Determines if Integration Points should use Export Core instead of older WebAPI Export.&apos;, @valueType = &apos;bit&apos;, @initialValue = &apos;true&apos;, @createAsSystemArtifact = 1.
        /// </summary>
        internal static string AddReplaceWebAPIWithExportCoreSetting {
            get {
                return ResourceManager.GetString("AddReplaceWebAPIWithExportCoreSetting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IF NOT EXISTS (SELECT * FROM [eddsdbo].[Configuration] WHERE [Section] = &apos;kCura.IntegrationPoints&apos; AND [Name] = &apos;WebAPIPath&apos;)
        ///BEGIN
        ///	INSERT INTO [eddsdbo].[Configuration] VALUES (&apos;kCura.IntegrationPoints&apos;, &apos;WebAPIPath&apos;, &apos;http://localhost/RelativityWebAPI/&apos;, &apos;&apos;, &apos;Relativity WebAPI URL for Relativity Integration Points&apos;)
        ///END
        ///ELSE
        ///BEGIN
        ///	UPDATE	[eddsdbo].[Configuration] 
        ///	SET			[Description] = &apos;Relativity WebAPI URL for Relativity Integration Points&apos;
        ///	WHERE		[Section] = &apos;kCura.IntegrationPoints&apos; AND [N [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string AddWebApiConfig {
            get {
                return ResourceManager.GetString("AddWebApiConfig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IF OBJECT_ID(N&apos;{0}.[{1}]&apos;,N&apos;U&apos;) IS NULL
        ///BEGIN
        ///	CREATE TABLE {0}.[{1}] 
        ///	([JobID] bigint,
        ///	[TotalRecords] int,
        ///	[ErrorRecords] int,
        ///	[ImportApiErrors] int,
        ///	[Completed] bit,
        ///	CONSTRAINT [PK_{1}] PRIMARY KEY CLUSTERED 
        ///	(
        ///		[JobID] ASC
        ///	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
        ///	) ON [PRIMARY]
        ///END
        ///
        ///IF (NOT EXISTS (SELECT * FROM {0}.[{1}] WHERE JobID = @jobID))
        ///BEGIN
        ///	insert into {0}.[{1}] ([JobID] ,[To [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateJobTrackingEntry {
            get {
                return ResourceManager.GetString("CreateJobTrackingEntry", resourceCulture);
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
        ///	AND				RF.[FileType]=0.
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
        ///   Looks up a localized string similar to SELECT [JobID] FROM {1}.
        /// </summary>
        internal static string GetJobIdsFromTrackingEntry {
            get {
                return ResourceManager.GetString("GetJobIdsFromTrackingEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT ot.ArtifactID, ot.DescriptorArtifactTypeID, ot.Name
        ///FROM [EDDSDBO].ObjectType ot WITH(NOLOCK)
        ///WHERE DescriptorArtifactTypeID in
        ///(
        ///	SELECT atg.ArtifactTypeID
        ///	FROM [EDDSDBO].[GroupUser] gu WITH(NOLOCK)
        ///	JOIN [EDDSDBO].[AccessControlListPermission]  acl WITH(NOLOCK) on gu.GroupArtifactID = acl.GroupID
        ///	JOIN [EDDSDBO].[Permission] p WITH(NOLOCK) on p.PermissionID = acl.PermissionID
        ///	JOIN [EDDSDBO].[ArtifactTypeGrouping] atg WITH(NOLOCK) on atg.ArtifactGroupingID = p.ArtifactGrouping
        ///	WHERE p.[T [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetObjectTypes {
            get {
                return ResourceManager.GetString("GetObjectTypes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SET ANSI_NULLS ON
        ///SET QUOTED_IDENTIFIER ON
        ///
        ///--Do cleanup first - delete old tables (over 72 hours old)
        ///DECLARE @table varchar(255) 
        ///DECLARE @dropCommand varchar(300) 
        ///DECLARE tableCursor CURSOR FOR 
        ///		SELECT &apos;{0}.&apos; + QUOTENAME(t.name) 
        ///		FROM {1}.[sys].[tables] AS t 
        ///		INNER JOIN {1}.[sys].[schemas] AS s 
        ///		ON t.[schema_id] = s.[schema_id] 
        ///		WHERE DATEDIFF(HOUR,t.create_date,GETUTCDATE())&gt;72
        ///		AND t.name LIKE &apos;RIP_JobTracker_%&apos;
        ///
        ///OPEN tableCursor 
        ///FETCH next FROM tableCursor INTO @table 
        ///
        ///W [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string RemoveEntryAndCheckBatchStatus {
            get {
                return ResourceManager.GetString("RemoveEntryAndCheckBatchStatus", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Update [eddsdbo].[IntegrationPoint]
        ///set LogErrors = 0
        ///where LogErrors IS NULL.
        /// </summary>
        internal static string SetBlankLogErrorsToNo {
            get {
                return ResourceManager.GetString("SetBlankLogErrorsToNo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE {0}.[{1}]
        ///	SET
        ///		[TotalRecords] += @total,
        ///		[ErrorRecords] += @errored,
        ///		[ImportApiErrors] += @importApiErrors
        ///	WHERE
        ///		[JobId] = @jobID
        ///
        ///SELECT
        ///	SUM(COALESCE([TotalRecords],0)) as [TotalRecords],
        ///	SUM(COALESCE([ErrorRecords],0)) as [ErrorRecords],
        ///	SUM(COALESCE([ImportApiErrors],0)) as [ImportApiErrors]
        ///FROM {0}.[{1}]
        ///.
        /// </summary>
        internal static string UpdateJobStatistics {
            get {
                return ResourceManager.GetString("UpdateJobStatistics", resourceCulture);
            }
        }
    }
}
