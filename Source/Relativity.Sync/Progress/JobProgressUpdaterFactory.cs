using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;
using Relativity.Sync.Utils.Workarounds;

namespace Relativity.Sync.Progress
{
    internal sealed class JobProgressUpdaterFactory : IJobProgressUpdaterFactory
    {
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IRdoGuidConfiguration _rdoGuidConfiguration;
        private readonly ISynchronizationConfiguration _synchronizationConfiguration;
        private readonly IDateTime _dateTime;
        private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
        private readonly IRipWorkarounds _ripWorkarounds;
        private readonly SyncJobParameters _syncJobParameters;
        private readonly IAPILog _logger;

        public JobProgressUpdaterFactory(
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin,
            IRdoGuidConfiguration rdoGuidConfiguration,
            ISynchronizationConfiguration synchronizationConfiguration,
            IDateTime dateTime,
            IJobHistoryErrorRepository jobHistoryErrorRepository,
            IRipWorkarounds ripWorkarounds,
            SyncJobParameters syncJobParameters,
            IAPILog logger)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _rdoGuidConfiguration = rdoGuidConfiguration;
            _synchronizationConfiguration = synchronizationConfiguration;
            _dateTime = dateTime;
            _jobHistoryErrorRepository = jobHistoryErrorRepository;
            _ripWorkarounds = ripWorkarounds;
            _syncJobParameters = syncJobParameters;
            _logger = logger;
        }

        public IJobProgressUpdater CreateJobProgressUpdater()
        {
            return new JobProgressUpdater(
                _serviceFactoryForAdmin,
                _rdoGuidConfiguration,
                _dateTime,
                _jobHistoryErrorRepository,
                _ripWorkarounds,
                _syncJobParameters,
                _logger,
                _synchronizationConfiguration.SourceWorkspaceArtifactId,
                _synchronizationConfiguration.JobHistoryArtifactId);
        }
    }
}
