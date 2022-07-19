using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Search;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
    internal class TagSavedSearchFolder : ITagSavedSearchFolder
    {
        private const string _DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME = "Integration Points";
        private readonly IDestinationServiceFactoryForUser _serviceFactoryForUser;
        private readonly IAPILog _logger;

        public TagSavedSearchFolder(IDestinationServiceFactoryForUser serviceFactoryForUser, IAPILog logger)
        {
            _serviceFactoryForUser = serviceFactoryForUser;
            _logger = logger;
        }

        public async Task<int> GetFolderIdAsync(int workspaceArtifactId)
        {
            _logger.LogVerbose("Getting Saved Search Folder Id for workspace {workspaceArtifactId}", workspaceArtifactId);
            try
            {
                SearchContainer existingFolder = await QuerySearchContainerAsync(workspaceArtifactId).ConfigureAwait(false);

                int folderArtifactId;
                if (existingFolder != null)
                {
                    folderArtifactId = existingFolder.ArtifactID;
                }
                else
                {
                    folderArtifactId = await CreateSearchContainerInRootAsync(workspaceArtifactId).ConfigureAwait(false);
                }

                return folderArtifactId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Saved Search Folder Id for workspace {workspaceId}", workspaceArtifactId);
                throw;
            }
        }

        private async Task<SearchContainer> QuerySearchContainerAsync(int workspaceId)
        {
            _logger.LogVerbose("Querying for Saved Search Folder named {name} in {workspaceId}", _DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME, workspaceId);
            Condition condition = new TextCondition(ClientFieldNames.Name, TextConditionEnum.EqualTo, _DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME);
            var sort = new Sort
            {
                Direction = SortEnum.Descending,
                Order = 0,
                FieldIdentifier = { Name = "ArtifactID" }
            };
            string queryString = condition.ToQueryString();
            var query = new Services.Query(queryString, new List<Sort> { sort });

            SearchContainerQueryResultSet result;
            using (var proxy = await _serviceFactoryForUser.CreateProxyAsync<ISearchContainerManager>().ConfigureAwait(false))
            {
                result = await proxy.QueryAsync(workspaceId, query).ConfigureAwait(false);
            }

            if (!result.Success)
            {
                throw new SyncException($"Failed to query Saved Search Folder named {_DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME} in workspace {workspaceId}: {result.Message}");
            }

            SearchContainer existingSearchContainer = null;
            if (result.Results.Any())
            {
                existingSearchContainer = result.Results[0].Artifact;
            }
            return existingSearchContainer;
        }

        private async Task<int> CreateSearchContainerInRootAsync(int workspaceId)
        {
            _logger.LogVerbose("Creating Saved Search Folder named {name} in {workspaceId}", _DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME, workspaceId);
            try
            {
                using (var proxy = await _serviceFactoryForUser.CreateProxyAsync<ISearchContainerManager>().ConfigureAwait(false))
                {
                    SearchContainer searchContainer = new SearchContainer
                    {
                        Name = _DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME,
                        ParentSearchContainer = { ArtifactID = 0 }
                    };
                    int folderArtifactId = await proxy.CreateSingleAsync(workspaceId, searchContainer).ConfigureAwait(false);
                    return folderArtifactId;
                }
            }
            catch (Exception ex)
            {
                throw new SyncException($"Failed to create Saved Search Folder with name {_DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME} in workspace {workspaceId}: {ex.Message}", ex);
            }
        }
    }
}
