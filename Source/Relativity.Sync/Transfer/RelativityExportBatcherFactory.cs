using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
	internal sealed class RelativityExportBatcherFactory : IRelativityExportBatcherFactory
	{
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISynchronizationConfiguration _configuration;

		public RelativityExportBatcherFactory(ISourceServiceFactoryForUser serviceFactory, ISynchronizationConfiguration configuration)
		{
			_serviceFactory = serviceFactory;
			_configuration = configuration;
		}

		public IRelativityExportBatcher CreateRelativityExportBatcher(IBatch batch)
		{
			return new RelativityExportBatcher(_serviceFactory, batch, _configuration.SourceWorkspaceArtifactId);
		}
	}
}