﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace kCura.ScheduleQueue.Core.Properties {
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("kCura.ScheduleQueue.Core.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to /****** 
        ///Script Date:		7/25/2016
        ///Script Creator:		Sorawit Amornborvornwong
        ///Script Description:	Adding cancel state column onto the schedule queue table.
        ///******/
        ///
        ///USE [EDDS]
        ///
        ///SET ANSI_NULLS ON
        ///SET QUOTED_IDENTIFIER ON
        ///
        ///IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N&apos;[eddsdbo].[{0}]&apos;) AND type in (N&apos;U&apos;))
        ///BEGIN
        ///	RAISERROR(&apos;{0} does not exist&apos;, 18, 1)
        ///END
        ///
        ///IF NOT EXISTS(
        ///    SELECT *
        ///    FROM sys.columns 
        ///    WHERE Name      = N&apos;StopState&apos;
        ///      AND Object_ID = Object_ [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string AddStopStateColumnToQueueTable {
            get {
                return ResourceManager.GetString("AddStopStateColumnToQueueTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to --If an agent was deleted while a job was running and not completed, we remove the agent lock
        ///--from the job so that another agent can pick it up.
        ///
        ///DECLARE @agentArtifactIds TABLE(ArtifactId int)
        ///
        ///INSERT INTO @agentArtifactIds
        ///SELECT A.[ArtifactID]
        ///FROM [eddsdbo].[Agent] as A WITH(NOLOCK)
        ///INNER JOIN [eddsdbo].[AgentType] as AT WITH(NOLOCK)
        ///ON A.[AgentTypeArtifactID] = AT.[ArtifactID]
        ///WHERE AT.[Guid] = @agentGuid
        ///
        ///UPDATE [eddsdbo].[{0}] WITH(UPDLOCK, READPAST, ROWLOCK)
        ///SET [LockedByAgentID] = NU [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CleanupJobQueueTable {
            get {
                return ResourceManager.GetString("CleanupJobQueueTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SET ANSI_NULLS ON
        ///SET QUOTED_IDENTIFIER ON
        ///
        ///IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N&apos;[eddsdbo].[{0}]&apos;) AND type in (N&apos;U&apos;))
        ///BEGIN
        ///	CREATE TABLE [eddsdbo].[{0}](
        ///		[ID] [bigint] IDENTITY(1,1) NOT NULL,
        ///		[JobID] [bigint] NOT NULL,
        ///		[RootJobID] [bigint] NULL,
        ///		[ParentJobID] [bigint] NULL,
        ///		[TaskType] [nvarchar](255) NOT NULL,
        ///		[Status] [int] NOT NULL,
        ///		[AgentID] [int] NULL,
        ///		[WorkspaceID] [int] NULL,
        ///		[RelatedObjectArtifactID] [int] NULL,
        ///		[CreatedBy] [int]  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateJobLogTable {
            get {
                return ResourceManager.GetString("CreateJobLogTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /****** 
        ///Script Number:		1
        ///Script Date:		11/13/2014 10:10:00 
        ///Script Creator:		Art Kelenzon
        ///Script Description:	Creating schedule queue table and corresponding indexes
        ///******/
        ///USE [EDDS]
        ///
        ///SET ANSI_NULLS ON
        ///SET QUOTED_IDENTIFIER ON
        ///
        ///IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N&apos;[eddsdbo].[{0}]&apos;) AND type in (N&apos;U&apos;))
        ///BEGIN
        ///CREATE TABLE [eddsdbo].[{0}](
        ///	[JobID] [bigint] IDENTITY(1,1) NOT NULL,
        ///	[RootJobID] [bigint] NULL,
        ///	[ParentJobID] [bigint] NULL,
        ///	[AgentTypeID] [in [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateQueueTable {
            get {
                return ResourceManager.GetString("CreateQueueTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DECLARE @job table
        ///(
        ///		[JobID] [bigint] NOT NULL,
        ///		[RootJobID] [bigint] NULL,
        ///		[ParentJobID] [bigint] NULL,
        ///		[AgentTypeID] [int] NOT NULL,
        ///		[LockedByAgentID] [int] NULL,
        ///		[WorkspaceID] [int] NOT NULL,
        ///		[RelatedObjectArtifactID] [int] NOT NULL,
        ///		[TaskType] [nvarchar](255) NOT NULL,
        ///		[NextRunTime] [datetime] NOT NULL,
        ///		[LastRunTime] [datetime] NULL,
        ///		[ScheduleRuleType] [nvarchar](max) NULL,
        ///		[ScheduleRule] [nvarchar](max) NULL,
        ///		[JobDetails] [nvarchar](max) NULL,
        ///		[JobFlags] [int]  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateScheduledJob {
            get {
                return ResourceManager.GetString("CreateScheduledJob", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DELETE FROM
        ///			[eddsdbo].[{0}] 
        ///WHERE
        ///			JobID = @JobID
        ///.
        /// </summary>
        internal static string DeleteJob {
            get {
                return ResourceManager.GetString("DeleteJob", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT TOP 1 
        ///			at.[ArtifactID] AS AgentTypeID,
        ///      at.[Name],
        ///      at.[Fullnamespace],
        ///      at.[Guid]
        ///FROM 
        ///			[eddsdbo].[AgentType]at WITH(NOLOCK)
        ///WHERE
        ///			at.[Guid] = @AgentGuid
        ///.
        /// </summary>
        internal static string GetAgentTypeInformation {
            get {
                return ResourceManager.GetString("GetAgentTypeInformation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///			[JobID]
        ///			,[RootJobID]
        ///			,[ParentJobID]
        ///			,[AgentTypeID]
        ///			,[LockedByAgentID]
        ///			,[WorkspaceID]
        ///			,[RelatedObjectArtifactID]
        ///			,[TaskType]
        ///			,[NextRunTime]
        ///			,[LastRunTime]
        ///			,[ScheduleRuleType]
        ///			,[ScheduleRule]
        ///			,[JobDetails]
        ///			,[JobFlags]
        ///			,[SubmittedDate]
        ///			,[SubmittedBy]
        ///			,[StopState]
        ///FROM
        ///			[eddsdbo].[{0}] WITH(NOLOCK)
        ///WHERE
        ///			JobID = @JobID
        ///.
        /// </summary>
        internal static string GetJobByID {
            get {
                return ResourceManager.GetString("GetJobByID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///			[JobID]
        ///			,[RootJobID]
        ///			,[ParentJobID]
        ///			,[AgentTypeID]
        ///			,[LockedByAgentID]
        ///			,[WorkspaceID]
        ///			,[RelatedObjectArtifactID]
        ///			,[TaskType]
        ///			,[NextRunTime]
        ///			,[LastRunTime]
        ///			,[ScheduleRuleType]
        ///			,[ScheduleRule]
        ///			,[JobDetails]
        ///			,[JobFlags]
        ///			,[SubmittedDate]
        ///			,[SubmittedBy]
        ///			,[StopState]
        ///FROM
        ///			[eddsdbo].[{0}] WITH(NOLOCK)
        ///WHERE
        ///			[WorkspaceID] = @WorkspaceID
        ///	AND 
        ///			[RelatedObjectArtifactID] = @RelatedObjectArtifactID
        ///	AND 
        ///			[TaskType] = @TaskTyp [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetJobByRelatedObjectID {
            get {
                return ResourceManager.GetString("GetJobByRelatedObjectID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IF EXISTS(SELECT TOP 1 JobID FROM [eddsdbo].[{0}] WHERE [LockedByAgentID] = @AgentID)
        ///BEGIN
        ///	--This Agent has stopped before finalizing this job previously
        ///	--So, pick it up again and finish it.
        ///	SELECT TOP (1)
        ///				[JobID],
        ///				[RootJobID],
        ///				[ParentJobID],
        ///				[AgentTypeID],
        ///				[LockedByAgentID],
        ///				[WorkspaceID],
        ///				[RelatedObjectArtifactID],
        ///				[TaskType],
        ///				[NextRunTime],
        ///				[LastRunTime],
        ///				[ScheduleRuleType],
        ///				[ScheduleRule],
        ///				[JobDetails],
        ///				[JobFlags],
        ///				[Subm [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetNextJob {
            get {
                return ResourceManager.GetString("GetNextJob", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INSERT INTO [eddsdbo].[{0}]
        ///		(
        ///			[JobID],
        ///			[RootJobID],
        ///			[ParentJobID],
        ///			[TaskType],
        ///			[Status],
        ///			[AgentID],
        ///			[WorkspaceID],
        ///			[RelatedObjectArtifactID],
        ///			[CreatedBy],
        ///			[CreatedOn],
        ///			[Details]
        ///		)
        ///	VALUES
        ///		(
        ///			@JobID
        ///			,@RootJobID
        ///			,@ParentJobID
        ///			,@TaskType
        ///			,@Status
        ///			,@AgentID
        ///			,@WorkspaceID 
        ///			,@RelatedObjectArtifactID 
        ///			,@CreatedBy
        ///			,GETUTCDATE()
        ///			,@Details
        ///		).
        /// </summary>
        internal static string InsertJobLogEntry {
            get {
                return ResourceManager.GetString("InsertJobLogEntry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE 
        ///				[eddsdbo].[{0}]
        ///SET
        ///				[LockedByAgentID] = NULL
        ///FROM 
        ///				[eddsdbo].[{0}] WITH (UPDLOCK, ROWLOCK)
        ///WHERE
        ///				[LockedByAgentID] = @AgentID.
        /// </summary>
        internal static string UnlockScheduledJob {
            get {
                return ResourceManager.GetString("UnlockScheduledJob", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE	
        ///					[eddsdbo].[{0}] 
        ///SET 
        ///					[NextRunTime] = @NextRunTime, 
        ///					[LockedByAgentID] = NULL 
        ///WHERE 
        ///					[JobID] = @JobID.
        /// </summary>
        internal static string UpdateScheduledJob {
            get {
                return ResourceManager.GetString("UpdateScheduledJob", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UPDATE	
        ///					[eddsdbo].[{0}] 
        ///SET 
        ///					[StopState] = @State
        ///WHERE 
        ///					[JobID] = @JobID.
        /// </summary>
        internal static string UpdateStopState {
            get {
                return ResourceManager.GetString("UpdateStopState", resourceCulture);
            }
        }
    }
}
