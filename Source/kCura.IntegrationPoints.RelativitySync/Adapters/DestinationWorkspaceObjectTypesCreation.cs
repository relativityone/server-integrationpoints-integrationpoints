using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.RelativitySourceRdo;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal sealed class DestinationWorkspaceObjectTypesCreation : IExecutor<IDestinationWorkspaceObjectTypesCreationConfiguration>,
		IExecutionConstrains<IDestinationWorkspaceObjectTypesCreationConfiguration>
	{
		private readonly IWindsorContainer _container;

		public DestinationWorkspaceObjectTypesCreation(IWindsorContainer container)
		{
			_container = container;
		}

		public Task<bool> CanExecuteAsync(IDestinationWorkspaceObjectTypesCreationConfiguration configuration, CancellationToken token)
		{
			bool shouldExecute = !configuration.IsSourceJobArtifactTypeIdSet || !configuration.IsSourceWorkspaceArtifactTypeIdSet;
			return Task.FromResult(shouldExecute);
		}

		public async Task ExecuteAsync(IDestinationWorkspaceObjectTypesCreationConfiguration configuration, CancellationToken token)
		{
			await Task.Yield();

			IHelper helper = _container.Resolve<IHelper>();
			IRepositoryFactory repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());
			IRelativitySourceRdoHelpersFactory relativitySourceRdoHelpersFactory = new RelativitySourceRdoHelpersFactory(repositoryFactory);
			IRelativitySourceJobRdoInitializer sourceJobRdoInitializer = new RelativitySourceJobRdoInitializer(helper, repositoryFactory, relativitySourceRdoHelpersFactory);
			IRelativitySourceWorkspaceRdoInitializer sourceWorkspaceRdoInitializer = new RelativitySourceWorkspaceRdoInitializer(helper, repositoryFactory, relativitySourceRdoHelpersFactory);

			int sourceWorkspaceArtifactTypeId = sourceWorkspaceRdoInitializer.InitializeWorkspaceWithSourceWorkspaceRdo(configuration.DestinationWorkspaceArtifactId);
			configuration.SetSourceWorkspaceArtifactTypeId(sourceWorkspaceArtifactTypeId);

			int sourceJobArtifactTypeId = sourceJobRdoInitializer.InitializeWorkspaceWithSourceJobRdo(configuration.DestinationWorkspaceArtifactId, sourceWorkspaceArtifactTypeId);
			configuration.SetSourceJobArtifactTypeId(sourceJobArtifactTypeId);
		}
	}
}