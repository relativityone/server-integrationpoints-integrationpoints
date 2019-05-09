using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Exceptions;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
	internal sealed class FolderPathRetriever : IFolderPathRetriever
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = 10;

		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncLog _logger;

		public FolderPathRetriever(ISourceServiceFactoryForUser serviceFactory, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
		}

		public async Task<IDictionary<int, string>> GetFolderPathsAsync(int workspaceArtifactId, ICollection<int> documentArtifactIds)
		{
			IDictionary<int, int> documentIdToFolderMap = await GetFolderIdsAsync(workspaceArtifactId, documentArtifactIds).ConfigureAwait(false);
			IDictionary<int, string> folderIdToFolderNameMap = await GetFolderNamesAsync(workspaceArtifactId, documentIdToFolderMap.Values.Distinct()).ConfigureAwait(false);

			return documentIdToFolderMap.ToDictionary(x => x.Key, x => folderIdToFolderNameMap[x.Value]);
		}

		private async Task<IDictionary<int, int>> GetFolderIdsAsync(int workspaceArtifactId, ICollection<int> documentArtifactIds)
		{
			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef {ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID},
					Condition = $"\"Artifact Id\" IN [{string.Join(" ,", documentArtifactIds)}]"
				};
				QueryResult result;
				try
				{
					const int start = 0;
					result = await objectManager.QueryAsync(workspaceArtifactId, request, start, documentArtifactIds.Count, new EmptyProgress<ProgressReport>()).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex, "Service call failed while querying for document parent folder IDs: {request}", request);
					throw new DocumentFolderRetrievalException($"Service call failed while querying for document parent folder IDs in workspace {workspaceArtifactId}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to query document parent folder IDs: {request}", request);
					throw new DocumentFolderRetrievalException($"Failed to query document parent folder IDs in workspace {workspaceArtifactId}", ex);
				}

				return result.Objects.ToDictionary(d => d.ArtifactID, d => d.ParentObject.ArtifactID);
			}
		}

		private async Task<IDictionary<int, string>> GetFolderNamesAsync(int workspaceArtifactId, IEnumerable<int> folderIds)
		{
			using (var folderManager = await _serviceFactory.CreateProxyAsync<IFolderManager>().ConfigureAwait(false))
			{
				try
				{
					List<FolderPath> result = await folderManager.GetFullPathListAsync(workspaceArtifactId, folderIds.ToList()).ConfigureAwait(false);
					return result.ToDictionary(f => f.ArtifactID, f => f.FullPath);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex, "Service call failed while getting folders in workspace {workspaceArtifactId}", workspaceArtifactId);
					throw new DocumentFolderRetrievalException($"Service call failed while getting folders in workspace {workspaceArtifactId}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to get folders in workspace: {workspaceArtifactId}", workspaceArtifactId);
					throw new DocumentFolderRetrievalException($"Failed to get folders in workspace {workspaceArtifactId}", ex);
				}
			}
		}
	}
}