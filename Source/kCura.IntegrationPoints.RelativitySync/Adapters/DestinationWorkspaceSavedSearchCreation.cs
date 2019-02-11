using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal sealed class DestinationWorkspaceSavedSearchCreation : IExecutor<IDestinationWorkspaceSavedSearchCreationConfiguration>,
		IExecutionConstrains<IDestinationWorkspaceSavedSearchCreationConfiguration>
	{
		private readonly IWindsorContainer _container;

		public DestinationWorkspaceSavedSearchCreation(IWindsorContainer container)
		{
			_container = container;
		}

		public Task<bool> CanExecuteAsync(IDestinationWorkspaceSavedSearchCreationConfiguration configuration, CancellationToken token)
		{
			bool shouldExecute = configuration.CreateSavedSearchForTags && !configuration.IsSavedSearchArtifactIdSet;
			return Task.FromResult(shouldExecute);
		}

		public async Task ExecuteAsync(IDestinationWorkspaceSavedSearchCreationConfiguration configuration, CancellationToken token)
		{
			await Task.Yield();

			IHelper helper = _container.Resolve<IHelper>();
			IRepositoryFactory repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());

			IMultiObjectSavedSearchCondition multiObjectSavedSearchCondition = new MultiObjectSavedSearchCondition();
			ITagSavedSearch tagSavedSearch = new TagSavedSearch(repositoryFactory, multiObjectSavedSearchCondition, helper);
			ITagSavedSearchFolder tagSavedSearchFolder = new TagSavedSearchFolder(repositoryFactory, helper);

			TagSavedSearchManager manager = new TagSavedSearchManager(tagSavedSearch, tagSavedSearchFolder);

			SourceJobDTO sourceJobDto = new SourceJobDTO
			{
				ArtifactId = configuration.SourceJobTagArtifactId,
				Name = configuration.SourceJobTagName
			};
			SourceWorkspaceDTO sourceWorkspaceDto = new SourceWorkspaceDTO
			{
				ArtifactId = configuration.SourceWorkspaceTagArtifactId
			};
			TagsContainer tagsContainer = new TagsContainer(sourceJobDto, sourceWorkspaceDto);
			int savedSearchArtifactId = manager.CreateSavedSearchForTagging(configuration.DestinationWorkspaceArtifactId, configuration.CreateSavedSearchForTags, tagsContainer);
			configuration.SetSavedSearchArtifactId(savedSearchArtifactId);
		}
	}
}