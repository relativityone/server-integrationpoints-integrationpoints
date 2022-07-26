using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
    internal sealed class FolderPathRetriever : IFolderPathRetriever
    {
        private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int) ArtifactType.Document;
        private const int _BATCH_SIZE = 100_000;

        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
        private readonly IAPILog _logger;

        public FolderPathRetriever(ISourceServiceFactoryForUser serviceFactoryForUser, IAPILog logger)
        {
            _serviceFactoryForUser = serviceFactoryForUser;
            _logger = logger;
        }

        public async Task<IDictionary<int, string>> GetFolderPathsAsync(int workspaceArtifactId, ICollection<int> documentArtifactIds)
        {
            if (documentArtifactIds == null || !documentArtifactIds.Any())
            {
                return new Dictionary<int, string>();
            }

            IDictionary<int, int> documentIdToFolderMap = await GetDocumentIdToFolderIdMapAsync(workspaceArtifactId, documentArtifactIds).ConfigureAwait(false);
            IDictionary<int, string> folderIdToFolderPathMap = await GetFolderIdToFolderPathMapAsync(workspaceArtifactId, documentIdToFolderMap.Values.Distinct()).ConfigureAwait(false);

            return documentIdToFolderMap.ToDictionary(x => x.Key, x => folderIdToFolderPathMap[x.Value]);
        }

        private async Task<IDictionary<int, int>> GetDocumentIdToFolderIdMapAsync(int workspaceArtifactId, ICollection<int> documentArtifactIds)
        {
            using (var objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                IEnumerable<IDictionary<int, int>> batchDocumentIdToFolderIdMaps = await documentArtifactIds
                    .SplitList(_BATCH_SIZE)
                    .SelectAsync(async x => await GetDocumentIdToFolderIdMapForBatchAsync(objectManager, workspaceArtifactId, x).ConfigureAwait(false))
                    .ConfigureAwait(false);

                IDictionary<int, int> documentIdToFolderIdMap = batchDocumentIdToFolderIdMaps
                    .SelectMany(x => x)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                return documentIdToFolderIdMap;
            }
        }

        private async Task<IDictionary<int, int>> GetDocumentIdToFolderIdMapForBatchAsync(IObjectManager objectManager, int workspaceArtifactId, ICollection<int> documentArtifactIds)
        {
            QueryRequest request = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID },
                Condition = $"\"ArtifactID\" IN [{string.Join(",", documentArtifactIds)}]"
            };
            QueryResult result;
            try
            {
                const int start = 0;
                result = await objectManager.QueryAsync(workspaceArtifactId, request, start, documentArtifactIds.Count).ConfigureAwait(false);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Service call failed while querying for document parent folder IDs: {request}", request);
                throw new SyncKeplerException($"Service call failed while querying for document parent folder IDs in workspace {workspaceArtifactId}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to query document parent folder IDs: {request}", request);
                throw new SyncKeplerException($"Failed to query document parent folder IDs in workspace {workspaceArtifactId}", ex);
            }

            IDictionary<int, int> documentIdToFolderId = result.Objects.ToDictionary(d => d.ArtifactID, d => d.ParentObject.ArtifactID);
            return documentIdToFolderId;
        }

        private async Task<IDictionary<int, string>> GetFolderIdToFolderPathMapAsync(int workspaceArtifactId, IEnumerable<int> folderIds)
        {
            using (var folderManager = await _serviceFactoryForUser.CreateProxyAsync<IFolderManager>().ConfigureAwait(false))
            {
                List<FolderPath> result;
                List<int> foldersWithIds = folderIds.ToList(); 
                try
                {
                    result = await folderManager.GetFullPathListAsync(workspaceArtifactId, foldersWithIds).ConfigureAwait(false);
                }
                catch (ServiceException ex)
                {
                    _logger.LogError(ex, "Service call failed while getting folders in workspace {workspaceArtifactId}", workspaceArtifactId);
                    throw new SyncKeplerException($"Service call failed while getting folders in workspace {workspaceArtifactId}", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get folders in workspace: {workspaceArtifactId}", workspaceArtifactId);
                    throw new SyncKeplerException($"Failed to get folders in workspace {workspaceArtifactId}", ex);
                }
                if (result.Count != foldersWithIds.Count)
                {
                    List<int> differentValues = foldersWithIds.Except(result.Select(x => x.ArtifactID)).ToList();
                    const int maxSubset = 50;
                    int subsetCount = Math.Min(differentValues.Count, maxSubset);
                    string subsetArtifactIds = string.Join(",", differentValues.Take(subsetCount));
                    _logger.LogWarning($"Could not find folders with IDs {subsetArtifactIds} in workspace {workspaceArtifactId}.");
                }
                return result.ToDictionary(f => f.ArtifactID, f => RemoveUnnecessarySpaces(f.FullPath));
            }
        }

        private static string RemoveUnnecessarySpaces(string folderPath)
        {
            return folderPath.TrimEnd().Replace(" \\ ", "\\");
        }
    }
}
