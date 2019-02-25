using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal sealed class DestinationWorkspaceTagsCreation : IExecutor<IDestinationWorkspaceTagsCreationConfiguration>, IExecutionConstrains<IDestinationWorkspaceTagsCreationConfiguration>
	{
		private readonly IWindsorContainer _container;

		public DestinationWorkspaceTagsCreation(IWindsorContainer container)
		{
			_container = container;
		}

		public Task<bool> CanExecuteAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			bool shouldExecute = !configuration.IsSourceWorkspaceTagSet || !configuration.IsSourceJobTagSet;
			return Task.FromResult(shouldExecute);
		}

		public async Task ExecuteAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
		{
			await Task.Yield();

			IHelper helper = _container.Resolve<IHelper>();
			IRepositoryFactory repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());
			ISourceWorkspaceManager sourceWorkspaceManager = new SourceWorkspaceManager(repositoryFactory, helper);
			ISourceJobManager sourceJobManager = new SourceJobManager(repositoryFactory, helper);

			SourceWorkspaceDTO sourceWorkspaceDto = sourceWorkspaceManager.CreateSourceWorkspaceDto(configuration.DestinationWorkspaceArtifactId, configuration.SourceWorkspaceArtifactId, null,
				configuration.SourceWorkspaceArtifactTypeId);
			configuration.SetSourceWorkspaceTag(sourceWorkspaceDto.ArtifactId, sourceWorkspaceDto.Name);

			SourceJobDTO sourceJobDto = sourceJobManager.CreateSourceJobDto(configuration.SourceWorkspaceArtifactId, configuration.DestinationWorkspaceArtifactId, configuration.JobArtifactId,
				sourceWorkspaceDto.ArtifactId, configuration.SourceJobArtifactTypeId);
			configuration.SetSourceJobTag(sourceJobDto.ArtifactId, sourceJobDto.Name);
		}
	}
}