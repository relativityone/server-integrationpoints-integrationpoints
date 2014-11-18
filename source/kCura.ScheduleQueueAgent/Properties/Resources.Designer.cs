﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace kCura.ScheduleQueueAgent.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("kCura.ScheduleQueueAgent.Properties.Resources", typeof(Resources).Assembly);
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
        ///Script Number:		1
        ///Script Date:		11/13/2014 10:10:00 
        ///Script Creator:		Art Kelenzon
        ///Script Description:	Creating Schedule Queue table and corresponding indexes
        ///******/
        ///USE [EDDS]
        ///GO
        ///SET ANSI_NULLS ON
        ///GO
        ///SET QUOTED_IDENTIFIER ON
        ///GO
        ///IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N&apos;[eddsdbo].[{0}]&apos;) AND type in (N&apos;U&apos;))
        ///BEGIN
        ///CREATE TABLE [eddsdbo].[{0}](
        ///	[JobID] [bigint] IDENTITY(1,1) NOT NULL,
        ///	[AgentTypeID] [int] NOT NULL,
        ///	[Status] [int] NOT NULL,
        ///	[LockedByA [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateQueueTable {
            get {
                return ResourceManager.GetString("CreateQueueTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT TOP 1 
        ///			a.[ArtifactID] AS AgentID,
        ///			at.[ArtifactID] AS AgentTypeID,
        ///      [Name],
        ///      [Fullnamespace],
        ///      [Guid]
        ///FROM 
        ///			[eddsdbo].[AgentType]at WITH(NOLOCK)
        ///JOIN
        ///			[eddsdbo].[Agent]a WITH(NOLOCK) ON at.ArtifactID=a.AgentTypeArtifactID
        ///WHERE
        ///			a.ArtifactID = @AgentID.
        /// </summary>
        internal static string GetAgentInformation {
            get {
                return ResourceManager.GetString("GetAgentInformation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IF EXISTS(SELECT TOP 1 JobID FROM [eddsdbo].[MethodQueue] WHERE [AgentID]=@AgentID)
        ///BEGIN
        ///	--This Agent has stopped before finalizing this job previously
        ///	--So, pick it up again and finish it.
        ///	SELECT TOP (1)
        ///				[JobID],
        ///				[AgentTypeID],
        ///				[Status],
        ///				[LockedByAgentID],
        ///				[WorkspaceID],
        ///				[RelatedObjectArtifactID],
        ///				[TaskType],
        ///				[NextRunTime],
        ///				[LastRunTime],
        ///				[ScheduleRules],
        ///				[JobDetail],
        ///				[JobFlags],
        ///				[SubmittedDate],
        ///				[SubmittedBy]
        ///	FROM
        ///				[eddsdb [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetNextJob {
            get {
                return ResourceManager.GetString("GetNextJob", resourceCulture);
            }
        }
    }
}
