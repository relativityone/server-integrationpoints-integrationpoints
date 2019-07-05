using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync
{
	internal sealed class JobProgressUpdaterFactory : IJobProgressUpdaterFactory
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly ISynchronizationConfiguration _synchronizationConfiguration;
		private readonly ISyncLog _logger;

		public JobProgressUpdaterFactory(ISourceServiceFactoryForAdmin serviceFactory, ISynchronizationConfiguration synchronizationConfiguration, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_synchronizationConfiguration = synchronizationConfiguration;
			_logger = logger;
		}

		public IJobProgressUpdater CreateJobProgressUpdater()
		{
			return new JobProgressUpdater(_serviceFactory, _synchronizationConfiguration.SourceWorkspaceArtifactId, _synchronizationConfiguration.JobHistoryArtifactId, _logger);
		}
	}
}