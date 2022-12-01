namespace Relativity.Sync.Storage
{
    /// <summary>
    /// Used to carry the necessary parameters required for creating a new Progress RDO for a sync job.
    /// </summary>
    internal sealed class CreateProgressDto
    {
        /// <summary>
        /// The name of the step for which progress is being reported.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The order of the current step in the process.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// The status of the current step.
        /// </summary>
        public SyncJobStatus Status { get; }

        /// <summary>
        /// The sync configuration artifact ID to which the step is linked.
        /// </summary>
        public int SyncConfigurationArtifactId { get; }

        /// <summary>
        /// The workspace artifact ID of the executing step.
        /// </summary>
        public int WorkspaceArtifactId { get; }

        /// <inheritdoc />
        public CreateProgressDto(string name, int order, SyncJobStatus status, int syncConfigurationArtifactId, int workspaceArtifactId)
        {
            Name = name;
            Order = order;
            Status = status;
            SyncConfigurationArtifactId = syncConfigurationArtifactId;
            WorkspaceArtifactId = workspaceArtifactId;
        }
    }
}
