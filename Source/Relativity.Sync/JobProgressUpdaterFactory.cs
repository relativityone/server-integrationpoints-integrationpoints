using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Utils;

namespace Relativity.Sync
{
    internal sealed class JobProgressUpdaterFactory : IJobProgressUpdaterFactory
    {
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
        private readonly IRdoGuidConfiguration _rdoGuidConfiguration;
        private readonly ISynchronizationConfiguration _synchronizationConfiguration;
        private readonly IDateTime _dateTime;
        private readonly IAPILog _logger;

		public JobProgressUpdaterFactory(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IRdoGuidConfiguration rdoGuidConfiguration, ISynchronizationConfiguration synchronizationConfiguration, IDateTime dateTime, IAPILog logger)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _rdoGuidConfiguration = rdoGuidConfiguration;
            _synchronizationConfiguration = synchronizationConfiguration;
            _dateTime = dateTime;
            _logger = logger;
        }

        public IJobProgressUpdater CreateJobProgressUpdater()
        {
			return new JobProgressUpdater(_serviceFactoryForAdmin, _rdoGuidConfiguration, _synchronizationConfiguration.SourceWorkspaceArtifactId, _synchronizationConfiguration.JobHistoryArtifactId, _dateTime, _logger);
        }
    }
}
