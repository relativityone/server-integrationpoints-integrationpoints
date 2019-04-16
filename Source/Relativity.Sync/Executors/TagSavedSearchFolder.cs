﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services;
using Relativity.Services.Search;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal class TagSavedSearchFolder : ITagSavedSearchFolder
	{
		private const string _DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME = "Integration Points";
		private readonly IDestinationServiceFactoryForUser _serviceFactoryForUser;
		private readonly ISyncLog _logger;

		public TagSavedSearchFolder(IDestinationServiceFactoryForUser serviceFactoryForUser, ISyncLog logger)
		{
			_serviceFactoryForUser = serviceFactoryForUser;
			_logger = logger;
		}

		public async Task<int> GetFolderIdAsync(int workspaceArtifactId)
		{
			_logger.LogVerbose("Getting Saved Search Folder Id for workspace {workspaceArtifactId}", workspaceArtifactId);
			try
			{
				SearchContainer existingFolder = await QuerySearchContainerAsync(workspaceArtifactId, _DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME).ConfigureAwait(false);

				if (existingFolder != null)
				{
					return existingFolder.ArtifactID;
				}

				return await CreateSearchContainerInRootAsync(workspaceArtifactId, _DESTINATION_WORKSPACE_SAVED_SEARCH_FOLDER_NAME).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get Saved Search Folder Id for workspace {workspaceId}", workspaceArtifactId);
				throw;
			}
		}

		private async Task<SearchContainer> QuerySearchContainerAsync(int workspaceId, string name)
		{
			_logger.LogVerbose("Querying for Saved Search Folder named {name} in {workspaceId}", name, workspaceId);
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
				result = await proxy.QueryAsync(workspaceId, query).ConfigureAwait(false);
			}

			if (!result.Success)
			{
				throw new SyncException($"Failed to query Saved Search Folder named {name} in workspace {workspaceId}: {result.Message}");
			}
			if (result.Results.Count > 0)
			{
				return result.Results[0].Artifact;
			}
			return null;
		}

		private async Task<int> CreateSearchContainerInRootAsync(int workspaceId, string name)
		{
			_logger.LogVerbose("Creating Saved Search Folder named {name} in {workspaceId}", name, workspaceId);
			try
			{
				using (var proxy = await _serviceFactoryForUser.CreateProxyAsync<ISearchContainerManager>().ConfigureAwait(false))
				{
					SearchContainer searchContainer = new SearchContainer
					{
						Name = name,
						ParentSearchContainer = { ArtifactID = 0 }
					};
					return await proxy.CreateSingleAsync(workspaceId, searchContainer).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				throw new SyncException($"Failed to create Saved Search Folder with name {name} in workspace {workspaceId}: {ex.Message}", ex);
			}
		}
	}
}