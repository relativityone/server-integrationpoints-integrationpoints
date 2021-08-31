using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;

namespace Relativity.Sync.Storage
{
    internal sealed class Batch : IBatch
    {
        private readonly int _workspaceArtifactId;
        private readonly IRdoManager _rdoManager;
        private readonly ISourceServiceFactoryForAdmin _serviceFactory;

        private static readonly Guid TransferredItemsCountGuid = new Guid(SyncBatchGuids.TransferredItemsCountGuid);
        private static readonly Guid FailedItemsCountGuid = new Guid(SyncBatchGuids.FailedItemsCountGuid);

        private static readonly Guid TotalDocumentsCountGuid = new Guid(SyncBatchGuids.TotalDocumentsCountGuid);

        private static readonly Guid TransferredDocumentsCountGuid =
            new Guid(SyncBatchGuids.TransferredDocumentsCountGuid);

        private static readonly Guid FailedDocumentsCountGuid = new Guid(SyncBatchGuids.FailedDocumentsCountGuid);

        private static readonly Guid MetadataBytesTransferredGuid =
            new Guid(SyncBatchGuids.MetadataBytesTransferredGuid);

        private static readonly Guid FilesBytesTransferredGuid = new Guid(SyncBatchGuids.FilesBytesTransferredGuid);
        private static readonly Guid TotalBytesTransferredGuid = new Guid(SyncBatchGuids.TotalBytesTransferredGuid);

        private static readonly Guid StartingIndexGuid = new Guid(SyncBatchGuids.StartingIndexGuid);
        private static readonly Guid StatusGuid = new Guid(SyncBatchGuids.StatusGuid);
        private static readonly Guid TaggedDocumentsCountGuid = new Guid(SyncBatchGuids.TaggedDocumentsCountGuid);

        internal const string _PARENT_OBJECT_FIELD_NAME = "SyncConfiguration";
        internal static readonly Guid BatchObjectTypeGuid = new Guid(SyncBatchGuids.SyncBatchObjectTypeGuid);

        private SyncBatchRdo _batchRdo = new SyncBatchRdo();

        private Batch(IRdoManager rdoManager, ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId)
        {
            _rdoManager = rdoManager;
            _workspaceArtifactId = workspaceArtifactId;
            _serviceFactory = serviceFactory;
        }

        public int ArtifactId => _batchRdo.ArtifactId;

        public int TotalDocumentsCount => _batchRdo.TotalDocumentsCount;

        public int TransferredDocumentsCount => _batchRdo.TransferredDocumentsCount;

        public int FailedDocumentsCount => _batchRdo.FailedDocumentsCount;

        public int TransferredItemsCount => _batchRdo.TransferredItemsCount;

        public int FailedItemsCount => _batchRdo.FailedItemsCount;

        public long MetadataBytesTransferred => _batchRdo.MetadataTransferredBytes;

        public long FilesBytesTransferred => _batchRdo.FilesTransferredBytes;

        public long TotalBytesTransferred => _batchRdo.TotalTransferredBytes;

        public int TaggedDocumentsCount => _batchRdo.TaggedDocumentsCount;

        public int StartingIndex => _batchRdo.StartingIndex;

        public BatchStatus Status => ParseStatus(_batchRdo.Status);

        public async Task SetFailedItemsCountAsync(int failedItemsCount)
        {
            await UpdateFieldValueAsync(x => x.FailedItemsCount, failedItemsCount).ConfigureAwait(false);
        }

        public async Task SetTransferredItemsCountAsync(int transferredItemsCount)
        {
            await UpdateFieldValueAsync(x => x.TransferredItemsCount, transferredItemsCount).ConfigureAwait(false);
        }

        public async Task SetTransferredDocumentsCountAsync(int transferredDocumentsCount)
        {
            await UpdateFieldValueAsync(x => x.TransferredDocumentsCount, transferredDocumentsCount)
                .ConfigureAwait(false);
        }

        public async Task SetFailedDocumentsCountAsync(int failedDocumentsCount)
        {
            await UpdateFieldValueAsync(x => x.FailedDocumentsCount, failedDocumentsCount).ConfigureAwait(false);
        }

        public async Task SetStatusAsync(BatchStatus status)
        {
            string statusDescription = status.GetDescription();
            await UpdateFieldValueAsync(x => x.Status, statusDescription).ConfigureAwait(false);
        }

        public async Task SetMetadataBytesTransferredAsync(long metadataBytesTransferred)
        {
            await UpdateFieldValueAsync(x => x.MetadataTransferredBytes, metadataBytesTransferred)
                .ConfigureAwait(false);
        }

        public async Task SetFilesBytesTransferredAsync(long filesBytesTransferred)
        {
            await UpdateFieldValueAsync(x => x.FilesTransferredBytes, filesBytesTransferred).ConfigureAwait(false);
        }

        public async Task SetTotalBytesTransferredAsync(long totalBytesTransferred)
        {
            await UpdateFieldValueAsync(x => x.TotalTransferredBytes, totalBytesTransferred).ConfigureAwait(false);
        }

        public async Task SetTaggedDocumentsCountAsync(int taggedDocumentsCount)
        {
            await UpdateFieldValueAsync(x => x.TaggedDocumentsCount, taggedDocumentsCount).ConfigureAwait(false);
        }

        public async Task SetStartingIndexAsync(int newStartIndex)
        {
            await UpdateFieldValueAsync(x => x.StartingIndex, newStartIndex).ConfigureAwait(false);

        }

        private BatchStatus ParseStatus(string description)
        {
            return description.GetEnumFromDescription<BatchStatus>();
        }

        private async Task CreateAsync(int syncConfigurationArtifactId, int totalDocumentsCount, int startingIndex)
        {
            _batchRdo.TotalDocumentsCount = totalDocumentsCount;
            _batchRdo.StartingIndex = startingIndex;

            await _rdoManager.CreateAsync(_workspaceArtifactId, _batchRdo, syncConfigurationArtifactId)
                .ConfigureAwait(false);
        }

        private async Task<bool> ReadLastAsync(int syncConfigurationArtifactId)
        {
            using (IObjectManager objectManager =
                await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                QueryRequest queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = BatchObjectTypeGuid
                    },
                    Condition = $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}",
                    IncludeNameInQueryResult = true,
                    Sorts = new[]
                    {
                        new Sort
                        {
                            FieldIdentifier = new FieldRef
                            {
                                Guid = StartingIndexGuid
                            },
                            Direction = SortEnum.Descending
                        }
                    }
                };

                QueryResult result = await objectManager
                    .QueryAsync(_workspaceArtifactId, queryRequest, start: 1, length: 1).ConfigureAwait(false);

                if (result.TotalCount == 0)
                {
                    return false;
                }

                _batchRdo = await _rdoManager.GetAsync<SyncBatchRdo>(_workspaceArtifactId, result.Objects[0].ArtifactID)
                    .ConfigureAwait(false);

                return true;
            }
        }

        private async Task<bool> ReadNextAsync(int syncConfigurationArtifactId, int startingIndex)
        {
            using (IObjectManager objectManager =
                await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                QueryRequest queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = BatchObjectTypeGuid
                    },
                    Condition =
                        $"('{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}) AND ('{StartingIndexGuid}' > {startingIndex})",
                    IncludeNameInQueryResult = true,
                    Sorts = new[]
                    {
                        new Sort
                        {
                            FieldIdentifier = new FieldRef
                            {
                                Guid = StartingIndexGuid
                            },
                            Direction = SortEnum.Ascending
                        }
                    }
                };

                QueryResult result = await objectManager
                    .QueryAsync(_workspaceArtifactId, queryRequest, start: 1, length: 1).ConfigureAwait(false);

                if (result.TotalCount == 0)
                {
                    return false;
                }

                _batchRdo = await _rdoManager.GetAsync<SyncBatchRdo>(_workspaceArtifactId, result.Objects[0].ArtifactID)
                    .ConfigureAwait(false);
                
                return true;
            }
        }

        private async Task InitializeAsync(int artifactId)
        {
            _batchRdo = await _rdoManager.GetAsync<SyncBatchRdo>(_workspaceArtifactId, artifactId)
                .ConfigureAwait(false);

            if (_batchRdo is null)
            {
                throw new SyncException($"Batch ArtifactID: {artifactId} not found.");
            }
        }
            
        private async Task<IEnumerable<int>> GetAllBatchesIdsToExecuteAsync(int syncConfigurationArtifactId)
        {
            Task<IEnumerable<int>> getPausedBatches =
                ReadBatchesIdsWithStatusAsync(syncConfigurationArtifactId, BatchStatus.Paused);

            Task<IEnumerable<int>> getNewBatches =
                ReadBatchesIdsWithStatusAsync(syncConfigurationArtifactId, BatchStatus.New);

            IEnumerable<int>[] allBatches = await Task.WhenAll(getPausedBatches, getNewBatches).ConfigureAwait(false);

            return allBatches.SelectMany(x => x);
        }

        private async Task<IEnumerable<IBatch>> GetAllSuccessfullyExecutedBatchesAsync(int syncConfigurationArtifactId)
        {
            Task<IEnumerable<IBatch>> getCompletedBatches =
                ReadBatchesWithStatusAsync(syncConfigurationArtifactId, BatchStatus.Completed);

            Task<IEnumerable<IBatch>> getCompletedWithErrorsBatches =
                ReadBatchesWithStatusAsync(syncConfigurationArtifactId, BatchStatus.CompletedWithErrors);

            IEnumerable<IBatch>[] allBatches = await Task.WhenAll(getCompletedBatches, getCompletedWithErrorsBatches)
                .ConfigureAwait(false);

            return allBatches.SelectMany(x => x);
        }

        private async Task<IEnumerable<int>> ReadBatchesIdsWithStatusAsync(int syncConfigurationArtifactId,
            BatchStatus batchStatus)
        {
            IEnumerable<int> batchIds = System.Array.Empty<int>();
            using (IObjectManager objectManager =
                await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                QueryRequest queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = BatchObjectTypeGuid
                    },
                    Condition =
                        $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId} AND '{StatusGuid}' == '{batchStatus.GetDescription()}'",
                    IncludeNameInQueryResult = true
                };

                QueryResult result = await objectManager
                    .QueryAsync(_workspaceArtifactId, queryRequest, start: 1, length: int.MaxValue)
                    .ConfigureAwait(false);
                if (result.TotalCount > 0)
                {
                    batchIds = result.Objects.Select(x => x.ArtifactID);
                }
            }

            return batchIds;
        }

        private async Task<IEnumerable<IBatch>> ReadBatchesWithStatusAsync(int syncConfigurationArtifactId,
            BatchStatus batchStatus)
        {
            var batches = new ConcurrentBag<IBatch>();
            using (IObjectManager objectManager =
                await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = BatchObjectTypeGuid
                    },
                    Condition =
                        $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId} AND '{StatusGuid}' == '{batchStatus.GetDescription()}'",
                    IncludeNameInQueryResult = true
                };

                QueryResultSlim result = await objectManager
                    .QuerySlimAsync(_workspaceArtifactId, queryRequest, start: 1, length: int.MaxValue)
                    .ConfigureAwait(false);
                if (result.TotalCount > 0)
                {
                    IEnumerable<int> batchIds = result.Objects.Select(x => x.ArtifactID);

                    Parallel.ForEach(batchIds, batchArtifactId =>
                    {
                        var batch = new Batch(_rdoManager, _serviceFactory, _workspaceArtifactId);
                        batch.InitializeAsync(batchArtifactId).ConfigureAwait(false).GetAwaiter().GetResult();
                        batches.Add(batch);
                    });
                }
            }

            return batches;
        }

        private async Task<IEnumerable<IBatch>> ReadAllAsync(int syncConfigurationArtifactId)
        {
            var batches = new ConcurrentBag<IBatch>();
            using (IObjectManager objectManager =
                await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = BatchObjectTypeGuid
                    },
                    Condition = $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}",
                    IncludeNameInQueryResult = true
                };

                QueryResultSlim result = await objectManager
                    .QuerySlimAsync(_workspaceArtifactId, queryRequest, start: 1, length: int.MaxValue)
                    .ConfigureAwait(false);
                if (result.TotalCount > 0)
                {
                    IEnumerable<int> batchIds = result.Objects.Select(x => x.ArtifactID);

                    Parallel.ForEach(batchIds, batchArtifactId =>
                    {
                        var batch = new Batch(_rdoManager, _serviceFactory, _workspaceArtifactId);
                        batch.InitializeAsync(batchArtifactId).ConfigureAwait(false).GetAwaiter().GetResult();
                        batches.Add(batch);
                    });
                }
            }

            return batches;
        }

        private async Task UpdateFieldValueAsync<TValue>(Expression<Func<SyncBatchRdo, TValue>> expression,
            TValue value)
        {
            await _rdoManager.SetValueAsync(_workspaceArtifactId, _batchRdo, expression, value).ConfigureAwait(false);
        }

        public static async Task<IBatch> CreateAsync(IRdoManager rdoManager,
            ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId,
            int totalDocumentsCount, int startingIndex)
        {
            Batch batch = new Batch(rdoManager, serviceFactory, workspaceArtifactId);
            await batch.CreateAsync(syncConfigurationArtifactId, totalDocumentsCount, startingIndex)
                .ConfigureAwait(false);
            return batch;
        }

        public static async Task<IBatch> GetAsync(IRdoManager rdoManager, ISourceServiceFactoryForAdmin serviceFactory,
            int workspaceArtifactId, int artifactId)
        {
            Batch batch = new Batch(rdoManager, serviceFactory, workspaceArtifactId);
            await batch.InitializeAsync(artifactId).ConfigureAwait(false);
            return batch;
        }

        public static async Task<IEnumerable<IBatch>> GetAllAsync(IRdoManager rdoManager,
            ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId)
        {
            Batch batch = new Batch(rdoManager, serviceFactory, workspaceArtifactId);
            IEnumerable<IBatch> batches = await batch.ReadAllAsync(syncConfigurationArtifactId).ConfigureAwait(false);
            return batches;
        }

        public static async Task<IBatch> GetLastAsync(IRdoManager rdoManager,
            ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId)
        {
            Batch batch = new Batch(rdoManager, serviceFactory, workspaceArtifactId);
            bool batchFound = await batch.ReadLastAsync(syncConfigurationArtifactId).ConfigureAwait(false);
            return batchFound ? batch : null;
        }

        public static async Task<IEnumerable<int>> GetAllBatchesIdsToExecuteAsync(IRdoManager rdoManager,
            ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationId)
        {
            var batch = new Batch(rdoManager, serviceFactory, workspaceArtifactId);
            IEnumerable<int> batchIds =
                await batch.GetAllBatchesIdsToExecuteAsync(syncConfigurationId).ConfigureAwait(false);
            return batchIds;
        }

        public static async Task<IEnumerable<IBatch>> GetAllSuccessfullyExecutedBatchesAsync(IRdoManager rdoManager,
            ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationId)
        {
            var batch = new Batch(rdoManager, serviceFactory, workspaceArtifactId);
            IEnumerable<IBatch> batches = await batch.GetAllSuccessfullyExecutedBatchesAsync(syncConfigurationId)
                .ConfigureAwait(false);
            return batches;
        }

        public static async Task<IBatch> GetNextAsync(IRdoManager rdoManager,
            ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId,
            int startingIndex)
        {
            Batch batch = new Batch(rdoManager, serviceFactory, workspaceArtifactId);
            bool batchFound =
                await batch.ReadNextAsync(syncConfigurationArtifactId, startingIndex).ConfigureAwait(false);
            return batchFound ? batch : null;
        }
    }
}