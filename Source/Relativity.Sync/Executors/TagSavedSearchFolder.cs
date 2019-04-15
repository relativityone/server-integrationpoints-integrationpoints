using System;
using System.Collections.Generic;
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
		private ISyncLog _logger;

		public TagSavedSearchFolder(IDestinationServiceFactoryForUser serviceFactoryForUser, ISyncLog logger)
		{
			_serviceFactoryForUser = serviceFactoryForUser;
			_logger = logger;
		}

		public async Task<int> GetFolderId(int workspaceArtifactId)
		{
			SearchContainer existingFolder = await QuerySearchContainer(workspaceArtifactId, _DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME).ConfigureAwait(false);

			if (existingFolder != null)
			{
				return existingFolder.ArtifactID;
			}

			return await CreateSearchContainerInRoot(workspaceArtifactId, _DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME).ConfigureAwait(false);
		}

		public async Task<SearchContainer> QuerySearchContainer(int workspaceId, string name)
		{
			Condition condition = new TextCondition(ClientFieldNames.Name, TextConditionEnum.EqualTo, name);
			var sort = new Sort
			{
				Direction = SortEnum.Descending,
				Order = 0,
				FieldIdentifier = { Name = "ArtifactID" }
			};
			var query = new Services.Query(condition.ToQueryString(), new List<Sort> { sort });

			SearchContainerQueryResultSet result;
			using (var proxy = await _serviceFactoryForUser.CreateProxyAsync<ISearchContainerManager>().ConfigureAwait(false))
			{
				result = proxy.QueryAsync(workspaceId, query).Result;
			}

			if (!result.Success)
			{
				throw new SyncException($"Failed to query Saved Search Folder {result.Message}");
			}
			if (result.Results.Count > 0)
			{
				return result.Results[0].Artifact;
			}
			return null;
		}

		public async Task<int> CreateSearchContainerInRoot(int workspaceId, string name)
		{
			using (var proxy = await _serviceFactoryForUser.CreateProxyAsync<ISearchContainerManager>().ConfigureAwait(false))
			{
				SearchContainer searchContainer = new SearchContainer
				{
					Name = name,
					ParentSearchContainer = { ArtifactID = 0 }
				};
				return proxy.CreateSingleAsync(workspaceId, searchContainer).Result;
			}
		}
	}
}