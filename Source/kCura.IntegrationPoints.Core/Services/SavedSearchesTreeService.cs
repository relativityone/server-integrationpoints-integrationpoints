using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Services.Search;
using Relativity.Services.User;
using SearchProviderCondition = Relativity.Services.Search.SearchProviderCondition;

namespace kCura.IntegrationPoints.Core.Services
{
	public class SavedSearchesTreeService : ISavedSearchesTreeService
	{
		private readonly ISearchContainerManager _searchContainerManager;
		private readonly ISavedSearchesTreeCreator _treeCreator;
		private readonly IRepositoryFactory _repositoryFactory;

		public SavedSearchesTreeService(IHelper helper, ISavedSearchesTreeCreator treeCreator, IRepositoryFactory repositoryFactory)
		{
			_searchContainerManager = helper.GetServicesManager().CreateProxy<ISearchContainerManager>(ExecutionIdentity.CurrentUser);
			_treeCreator = treeCreator;
			_repositoryFactory = repositoryFactory;
		}

		public JsTreeItemDTO GetSavedSearchesTree(int workspaceArtifactId)
		{
			IEnumerable<SavedSearchDTO> savedSearchDtos = _repositoryFactory.GetSavedSearchQueryRepository(workspaceArtifactId).RetrievePublicSavedSearches();

			List<int> searchContainersArtifactIds = savedSearchDtos.Select(x => x.ArtifactId).ToList();

			SearchContainerItemCollection searchContainterCollection = _searchContainerManager.GetSearchContainerTreeAsync(workspaceArtifactId, searchContainersArtifactIds).Result;

			IEnumerable<SavedSearchContainerItem> publicSearchContainerItemCollection = searchContainterCollection.SavedSearchContainerItems.Where(
				item => searchContainersArtifactIds.Contains(item.SavedSearch.ArtifactID));

			JsTreeItemDTO tree = _treeCreator.Create(searchContainterCollection.SearchContainerItems, publicSearchContainerItemCollection);

			return tree;
		}
	}
}