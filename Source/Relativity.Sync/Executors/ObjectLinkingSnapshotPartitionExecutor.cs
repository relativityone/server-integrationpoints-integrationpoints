using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal sealed class ObjectLinkingSnapshotPartitionExecutor : SnapshotPartitionExecutorBase
    {
        private readonly ISyncLog _logger;

        public ObjectLinkingSnapshotPartitionExecutor(IBatchRepository batchRepository, ISyncLog logger)
            : base(batchRepository, logger)
        {
            _logger = logger;
        }

        protected override void LogSnapshotPartitionsInformation(ISnapshotPartitionConfiguration configuration)
        {
            _logger.LogInformation(
                "Creating object linking snapshot partitions for source workspace (workspace artifact id: {sourceWorkspaceArtifactId})",
                configuration.SourceWorkspaceArtifactId);
        }
    }
}
