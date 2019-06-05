using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync
{
	internal sealed class JobProgressUpdaterFactory : IJobProgressUpdaterFactory
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly ISynchronizationConfiguration _synchronizationConfiguration;

		public JobProgressUpdaterFactory(ISourceServiceFactoryForAdmin serviceFactory, ISynchronizationConfiguration synchronizationConfiguration)
		{
			_serviceFactory = serviceFactory;
			_synchronizationConfiguration = synchronizationConfiguration;
		}

		public IJobProgressUpdater CreateJobProgressUpdater()
		{
			return new JobProgressUpdater(_serviceFactory, _synchronizationConfiguration.SourceWorkspaceArtifactId, _synchronizationConfiguration.JobHistoryArtifactId);
		}
	}
}