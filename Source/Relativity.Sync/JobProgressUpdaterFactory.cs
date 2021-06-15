using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using System.Collections.Generic;

namespace Relativity.Sync
{
	internal sealed class JobProgressUpdaterFactory : IJobProgressUpdaterFactory
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly IRdoGuidConfiguration _rdoGuidConfiguration;
		private readonly ISynchronizationConfiguration _synchronizationConfiguration;
		private readonly ISyncLog _logger;

		public JobProgressUpdaterFactory(ISourceServiceFactoryForAdmin serviceFactory, IRdoGuidConfiguration rdoGuidConfiguration, ISynchronizationConfiguration synchronizationConfiguration, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_rdoGuidConfiguration = rdoGuidConfiguration;
			_synchronizationConfiguration = synchronizationConfiguration;
			_logger = logger;
		}

		public IJobProgressUpdater CreateJobProgressUpdater()
		{
			return new JobProgressUpdater(_serviceFactory, _rdoGuidConfiguration, _synchronizationConfiguration.SourceWorkspaceArtifactId, _synchronizationConfiguration.JobHistoryArtifactId, _logger);
		}
	}
}