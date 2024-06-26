﻿using System;
using System.Reflection;

namespace Relativity.Sync
{
    /// <summary>
    /// Represents Sync job parameters
    /// </summary>
    public sealed class SyncJobParameters
    {
        /// <summary>
        /// Job ID
        /// </summary>
        public Guid JobID { get; set; }

        /// <summary>
        /// Sync Configuration Artifact ID
        /// </summary>
        public int SyncConfigurationArtifactId { get; }

        /// <summary>
        /// ID of a workspace where job was created
        /// </summary>
        public int WorkspaceId { get; }

        /// <summary>
        /// ID of a user who runs the job
        /// </summary>
        public int UserId { get; }

        /// <summary>
        /// Build version of Sync
        /// </summary>
        public string SyncBuildVersion { get; }

        /// <summary>
        /// Name of the Sync application
        /// </summary>
        public string SyncApplicationName { get; set; } = "Relativity.Sync";

        /// <summary>
        /// Id for trigger input
        /// </summary>
        public string TriggerId { get; set; } = "type";

        /// <summary>
        /// Value for RAW trigger input
        /// </summary>
        public string TriggerValue { get; set; } = "sync";

        /// <summary>
        /// Name for RAW trigger
        /// </summary>
        public string TriggerName { get; set; } = "relativity@on-new-documents-added";

        /// <summary>
        /// Workflow ID.
        /// </summary>
        public string WorkflowId { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SyncJobParameters(int syncConfigurationArtifactId, int workspaceId, int userId, Guid workflowId, Guid jobId)
        {
            SyncConfigurationArtifactId = syncConfigurationArtifactId;
            WorkspaceId = workspaceId;
            UserId = userId;
            SyncBuildVersion = GetVersion();
            WorkflowId = workflowId.ToString();
            JobID = jobId;
        }

        private static string GetVersion()
        {
            string ver;

            try
            {
                ver = typeof(SyncJobParameters).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            }
            catch (Exception)
            {
                ver = "[file version not specified]";
            }

            return ver;
        }
    }
}
