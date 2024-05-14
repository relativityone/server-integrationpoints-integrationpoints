using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Kepler.Snapshot;
using Relativity.Sync.Kepler.SyncBatch;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
    internal sealed class RelativityExportBatcherFactory : IRelativityExportBatcherFactory
    {
        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly ISynchronizationConfiguration _configuration;
        private readonly IAPILog _logger;

        public RelativityExportBatcherFactory(ISourceServiceFactoryForUser serviceFactoryForUser, ISnapshotRepository snapshotRepository, ISynchronizationConfiguration configuration, IAPILog logger)
        {
            _serviceFactoryForUser = serviceFactoryForUser;
            _snapshotRepository = snapshotRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public IRelativityExportBatcher CreateRelativityExportBatcher(IBatch batch)
        {
            return new RelativityExportBatcher(_serviceFactoryForUser, batch, _configuration.SourceWorkspaceArtifactId);
        }

        public IRelativityExportBatcher CreateRelativityExportBatchForTagging(SyncBatchDto batch)
        {
            return new RelativityExportBatcher(
                _snapshotRepository,
                batch,
                _configuration.SourceWorkspaceArtifactId,
                batch => batch.InitialStartingIndex,
                batch => batch.TotalDocumentsCount,
                _logger);
        }
    }
}
