namespace Relativity.Sync.Configuration
{
    internal interface IJobCleanupConfiguration : IConfiguration
    {
        int SourceWorkspaceArtifactId { get; }

        int SyncConfigurationArtifactId { get; }

        ExecutionResult SynchronizationExecutionResult { get; set; }
    }
}
