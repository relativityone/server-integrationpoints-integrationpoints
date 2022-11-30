using System;

namespace Relativity.Sync.SyncConfiguration
{
    /// <inheritdoc cref="ISyncContext"/>
    public class SyncContext : ISyncContext
    {
        /// <inheritdoc cref="ISyncContext"/>
        public int SourceWorkspaceId { get; }

        /// <inheritdoc cref="ISyncContext"/>
        public int DestinationWorkspaceId { get; }

        /// <inheritdoc cref="ISyncContext"/>
        public int JobHistoryId { get; }

        /// <inheritdoc />
        public string ExecutingApplication { get; }

        /// <inheritdoc />
        public Version ExecutingApplicationVersion { get; }

        /// <summary>
        /// Creates new instance of <see cref="SyncContext"/> class.
        /// </summary>
        /// <param name="sourceWorkspaceId">Specifies the source workspace Artifact ID.</param>
        /// <param name="destinationWorkspaceId">Specifies the destination workspace Artifact ID.</param>
        /// <param name="jobHistoryId">Specifies Job History Artifact ID.</param>
        /// <param name="executingApplication">Specifies name of executing application</param>
        /// <param name="executingApplicationVersion">Specifies version of executing application</param>
        public SyncContext(int sourceWorkspaceId, int destinationWorkspaceId, int jobHistoryId, string executingApplication, Version executingApplicationVersion)
        {
            SourceWorkspaceId = sourceWorkspaceId;
            DestinationWorkspaceId = destinationWorkspaceId;
            JobHistoryId = jobHistoryId;
            ExecutingApplication = executingApplication;
            ExecutingApplicationVersion = executingApplicationVersion;
        }

        /// <summary>
        /// Internal constructor to make testing easier
        /// </summary>
        internal SyncContext(int sourceWorkspaceId, int destinationWorkspaceId, int jobHistoryId)
            : this(sourceWorkspaceId, destinationWorkspaceId, jobHistoryId, "SyncTests", new Version()) { }
    }
}
