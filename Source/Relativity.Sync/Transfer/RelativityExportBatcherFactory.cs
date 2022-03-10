using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	internal sealed class RelativityExportBatcherFactory : IRelativityExportBatcherFactory
	{
		private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
		private readonly ISynchronizationConfiguration _configuration;

		public RelativityExportBatcherFactory(ISourceServiceFactoryForUser serviceFactoryForUser, ISynchronizationConfiguration configuration)
		{
			_serviceFactoryForUser = serviceFactoryForUser;
			_configuration = configuration;
		}

		public IRelativityExportBatcher CreateRelativityExportBatcher(IBatch batch)
		{
			return new RelativityExportBatcher(_serviceFactoryForUser, batch, _configuration.SourceWorkspaceArtifactId);
		}
	}
}