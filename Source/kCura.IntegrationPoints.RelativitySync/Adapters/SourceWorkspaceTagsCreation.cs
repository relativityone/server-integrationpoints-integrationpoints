using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal sealed class SourceWorkspaceTagsCreation : IExecutor<ISourceWorkspaceTagsCreationConfiguration>, IExecutionConstrains<ISourceWorkspaceTagsCreationConfiguration>
	{
		private readonly IWindsorContainer _container;

		public SourceWorkspaceTagsCreation(IWindsorContainer container)
		{
			_container = container;
		}

		public Task<bool> CanExecuteAsync(ISourceWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(!configuration.IsDestinationWorkspaceTagArtifactIdSet);
		}

		public async Task<ExecutionResult> ExecuteAsync(ISourceWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			await Task.Yield();

			IHelper helper = _container.Resolve<IHelper>();
			IAPILog logger = _container.Resolve<IAPILog>();
			IRepositoryFactory repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());

			IDestinationWorkspaceRepository destinationWorkspaceRepository = repositoryFactory.GetDestinationWorkspaceRepository(configuration.SourceWorkspaceArtifactId);
			IWorkspaceRepository workspaceRepository = repositoryFactory.GetWorkspaceRepository();
			IFederatedInstanceManager federatedInstanceManager = new FederatedInstanceManager(repositoryFactory);

			SourceWorkspaceTagCreator sourceWorkspaceTagCreator = new SourceWorkspaceTagCreator(destinationWorkspaceRepository, workspaceRepository, federatedInstanceManager, logger);
			int destinationWorkspaceTagArtifactId = sourceWorkspaceTagCreator.CreateDestinationWorkspaceTag(configuration.DestinationWorkspaceArtifactId, configuration.JobArtifactId, null);

			configuration.SetDestinationWorkspaceTagArtifactId(destinationWorkspaceTagArtifactId);

			return ExecutionResult.Success();
		}
	}
}