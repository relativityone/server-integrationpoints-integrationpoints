using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal sealed class JobCleanupConfiguration : IJobCleanupConfiguration
    {
        private readonly SyncJobParameters _syncJobParameters;

        public JobCleanupConfiguration(SyncJobParameters syncJobParameters)
        {
            _syncJobParameters = syncJobParameters;
        }

        public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
        public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;
        public ExecutionResult SynchronizationExecutionResult { get; set; } = new ExecutionResult(ExecutionStatus.None, string.Empty, null);
    }
}