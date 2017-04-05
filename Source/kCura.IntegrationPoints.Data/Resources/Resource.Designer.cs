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
        ///	insert into [eddsdbo].[Configuration] ([Section], [Name], [Value],  [MachineName], [Description])
        ///	SELECT TOP 1 
        ///		&apos;kCura.IntegrationPoints&apos; as [Section],
        ///		&apos;WebAPIPath&apos; as [Name],
        ///		value as [Value],
        ///		&apos;&apos; as [MachineName],
        ///		&apos;Relativity WebAPI URL for Relativity Integration Points&apos; as [Description]
        ///	FROM	[eddsdbo].[Configuration]
        ///	WHERE 
        ///				[Section] = &apos;kCura.EDDS [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string AddWebApiConfig {
            get {
                return ResourceManager.GetString("AddWebApiConfig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SET ANSI_NULLS ON
        ///SET QUOTED_IDENTIFIER ON
        ///
        ///--Do cleanup first - delete old tables (over 72 hours old)
        ///DECLARE @table varchar(255) 
        ///DECLARE @dropCommand varchar(300) 
        ///
        ///DECLARE tableCursor CURSOR FOR 
        ///		SELECT &apos;{0}.&apos;+QUOTENAME(t.name) 
        ///		FROM {1}.[sys].[tables] AS t 
        ///		INNER JOIN {1}.[sys].[schemas] AS s 
        ///		ON t.[schema_id] = s.[schema_id] 
        ///		WHERE DATEDIFF(HOUR,t.create_date,GETUTCDATE())&gt;72
        ///		AND t.name LIKE &apos;RIP_CustodianManager_%&apos;
        ///
        ///OPEN tableCursor 
        ///FETCH next FROM tableCursor INTO @table [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateCustodianManagerResourceTable {
            get {
                return ResourceManager.GetString("CreateCustodianManagerResourceTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IF OBJECT_ID(N&apos;{0}.[{1}]&apos;,N&apos;U&apos;) IS NULL
        ///BEGIN
        ///	CREATE TABLE {0}.[{1}] 
        ///	([JobID] bigint,
        ///	[TotalRecords] int,
        ///	[ErrorRecords] int,
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
        ///	insert into {0}.[{1}] ([JobID],[Completed]) values (@jobID,  [rest of string was truncated]&quot;;.
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
        ///UPDATE	{0}.[{1}]
        ///SET
        ///				[LockedByJobID]	= -1
        ///FROM 
        ///				{0}.[{1}] t1 
        ///JOIN
        ///				(
        ///					SELECT * FROM {0}.[{1}] WHERE NOT [LockedByJobID] IS NULL
        ///				) t2
        ///	ON		t1.[CustodianID] = t2.[CustodianID] AND t1.[ManagerID] = t2.[ManagerID] 
        ///WHERE
        ///				t1.[LockedByJobID] IS NULL
        ///				
        ///
        ///--get next batch
        ///UPDATE			{0}.[{1}]
        ///SET
        ///				[LockedByJobID]	= @JobID
        ///OUTPUT 
        ///				INSERTED.[CustodianID],
        ///				INSERTED.[ManagerID]
        ///FROM 
        ///				{0}.[{1}] t1
        ///WHERE
        ///				t1.[LockedByJobID] IS N [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetJobCustodianManagerLinks {
            get {
                return ResourceManager.GetString("GetJobCustodianManagerLinks", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT COUNT(JobID) FROM [{0}] WHERE (JobID = @RootJobID OR RootJobID=@RootJobID).
        /// </summary>
        internal static string GetJobsCount {
            get {
                return ResourceManager.GetString("GetJobsCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT ot.DescriptorArtifactTypeID, ot.Name
        ///FROM [EDDSDBO].ObjectType ot WITH(NOLOCK)
        ///WHERE DescriptorArtifactTypeID in
        ///(
        ///	SELECT atg.ArtifactTypeID
        ///	FROM [EDDSDBO].[GroupUser] gu WITH(NOLOCK)
        ///	JOIN [EDDSDBO].[AccessControlListPermission]  acl WITH(NOLOCK) on gu.GroupArtifactID = acl.GroupID
        ///	JOIN [EDDSDBO].[Permission] p WITH(NOLOCK) on p.PermissionID = acl.PermissionID
        ///	JOIN [EDDSDBO].[ArtifactTypeGrouping] atg WITH(NOLOCK) on atg.ArtifactGroupingID = p.ArtifactGrouping
        ///	WHERE p.[Type] = 1
        ///	AND  [rest of string was truncated]&quot;;.
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
        ///   Looks up a localized string similar to IF NOT EXISTS (SELECT * FROM [eddsdbo].[TOGGLE] WHERE [Name] = &apos;kCura.IntegrationPoints.Web.Toggles.UI.ShowImageImportToggle&apos;)
        ///BEGIN
        ///	INSERT INTO [eddsdbo].[Toggle] VALUES (&apos;kCura.IntegrationPoints.Web.Toggles.UI.ShowImageImportToggle&apos;, 1)
        ///END.
        /// </summary>
        internal static string ShowImageImportToggle {
            get {
                return ResourceManager.GetString("ShowImageImportToggle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE {0}.[{1}]
        ///	SET
        ///		[TotalRecords] = @total,
        ///		[ErrorRecords] = @errored
        ///	WHERE
        ///		[JobId] = @jobID
        ///
        ///SELECT
        ///	SUM(COALESCE([TotalRecords],0)) as [TotalRecords],
        ///	SUM(COALESCE([ErrorRecords],0)) as [ErrorRecords]
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
