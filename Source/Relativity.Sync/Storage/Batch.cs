﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Extensions;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;

namespace Relativity.Sync.Storage
{
    internal sealed class Batch : IBatch
    {
        private readonly int _workspaceArtifactId;
        private readonly IRdoManager _rdoManager;
        private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;

        private static readonly Guid StartingIndexGuid = new Guid(SyncBatchGuids.StartingIndexGuid);
        private static readonly Guid StatusGuid = new Guid(SyncBatchGuids.StatusGuid);

        internal const string _PARENT_OBJECT_FIELD_NAME = "SyncConfiguration";
        internal const string _EXPORT_RUN_ID_NAME = "ExportRunId";
        internal static readonly Guid BatchObjectTypeGuid = new Guid(SyncBatchGuids.SyncBatchObjectTypeGuid);

        private SyncBatchRdo _batchRdo = new SyncBatchRdo();

        private Batch(IRdoManager rdoManager, ISourceServiceFactoryForAdmin serviceFactoryForAdmin, int workspaceArtifactId)
        {
            _rdoManager = rdoManager;
            _workspaceArtifactId = workspaceArtifactId;
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
        }

        public int ArtifactId => _batchRdo.ArtifactId;

        public int TotalDocumentsCount => _batchRdo.TotalDocumentsCount;

        public int TransferredDocumentsCount => _batchRdo.TransferredDocumentsCount;

        public int FailedDocumentsCount => _batchRdo.FailedDocumentsCount;

        public Guid ExportRunId => _batchRdo.ExportRunId;

        public Guid BatchGuid => _batchRdo.BatchGuid;

        public int TransferredItemsCount => _batchRdo.TransferredItemsCount;

        public int FailedItemsCount => _batchRdo.FailedItemsCount;

        public long MetadataBytesTransferred => _batchRdo.MetadataTransferredBytes;

        public long FilesBytesTransferred => _batchRdo.FilesTransferredBytes;

        public long TotalBytesTransferred => _batchRdo.TotalTransferredBytes;

        public int TaggedDocumentsCount => _batchRdo.TaggedDocumentsCount;

        public int StartingIndex => _batchRdo.StartingIndex;

        public BatchStatus Status => _batchRdo.Status;

        public bool IsFinished => _batchRdo.Status.IsIn(BatchStatus.Completed, BatchStatus.CompletedWithErrors, BatchStatus.Cancelled, BatchStatus.Failed);

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
            await UpdateFieldValueAsync(x => x.Status, status).ConfigureAwait(false);
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

        private async Task CreateAsync(int syncConfigurationArtifactId, int totalDocumentsCount, int startingIndex, Guid exportRunId)
        {
            _batchRdo.TotalDocumentsCount = totalDocumentsCount;
            _batchRdo.StartingIndex = startingIndex;
            _batchRdo.ExportRunId = exportRunId;
            _batchRdo.Status = BatchStatus.New;
            _batchRdo.BatchGuid = Guid.NewGuid();

            await _rdoManager.CreateAsync(_workspaceArtifactId, _batchRdo, syncConfigurationArtifactId)
                .ConfigureAwait(false);
        }

        private async Task<bool> ReadLastAsync(int syncConfigurationArtifactId, Guid exportRunId)
        {
            using (IObjectManager objectManager =
                await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                QueryRequest queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = BatchObjectTypeGuid
                    },
                    Condition = $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId} AND '{_EXPORT_RUN_ID_NAME}' == '{exportRunId}'",
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

        private async Task<bool> ReadNextAsync(int syncConfigurationArtifactId, int startingIndex, Guid exportRunId)
        {
            using (IObjectManager objectManager =
                await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                QueryRequest queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = BatchObjectTypeGuid
                    },
                    Condition =
                        $"('{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}) AND ('{StartingIndexGuid}' > {startingIndex}) AND '{_EXPORT_RUN_ID_NAME}' == '{exportRunId}'",
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

        private async Task<IEnumerable<int>> GetAllBatchesIdsToExecuteAsync(int syncConfigurationArtifactId, Guid exportRunId)
        {
            Task<IEnumerable<int>> getPausedBatches =
                ReadBatchesIdsWithStatusAsync(syncConfigurationArtifactId, BatchStatus.Paused, exportRunId);

            Task<IEnumerable<int>> getNewBatches =
                ReadBatchesIdsWithStatusAsync(syncConfigurationArtifactId, BatchStatus.New, exportRunId);

            IEnumerable<int>[] allBatches = await Task.WhenAll(getPausedBatches, getNewBatches).ConfigureAwait(false);

            return allBatches.SelectMany(x => x);
        }

        private async Task<IEnumerable<IBatch>> GetAllSuccessfullyExecutedBatchesAsync(int syncConfigurationArtifactId, Guid exportRunId)
        {
            Task<IEnumerable<IBatch>> getCompletedBatches =
                ReadBatchesWithStatusAsync(syncConfigurationArtifactId, BatchStatus.Completed, exportRunId);

            Task<IEnumerable<IBatch>> getCompletedWithErrorsBatches =
                ReadBatchesWithStatusAsync(syncConfigurationArtifactId, BatchStatus.CompletedWithErrors, exportRunId);

            IEnumerable<IBatch>[] allBatches = await Task.WhenAll(getCompletedBatches, getCompletedWithErrorsBatches)
                .ConfigureAwait(false);

            return allBatches.SelectMany(x => x);
        }

        private async Task<IEnumerable<int>> ReadBatchesIdsWithStatusAsync(int syncConfigurationArtifactId,
            BatchStatus batchStatus, Guid exportRunId)
        {
            IEnumerable<int> batchIds = System.Array.Empty<int>();
            using (IObjectManager objectManager =
                await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                QueryRequest queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = BatchObjectTypeGuid
                    },
                    Condition =
                        $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId} AND '{StatusGuid}' == '{batchStatus.GetDescription()}' AND '{_EXPORT_RUN_ID_NAME}' == '{exportRunId}'",
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
            BatchStatus batchStatus, Guid exportRunId)
        {
            var batches = new ConcurrentBag<IBatch>();
            using (IObjectManager objectManager =
                await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = BatchObjectTypeGuid
                    },
                    Condition =
                        $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId} AND '{StatusGuid}' == '{batchStatus.GetDescription()}' AND '{_EXPORT_RUN_ID_NAME}' == '{exportRunId}'",
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
                        var batch = new Batch(_rdoManager, _serviceFactoryForAdmin, _workspaceArtifactId);
                        batch.InitializeAsync(batchArtifactId).ConfigureAwait(false).GetAwaiter().GetResult();
                        batches.Add(batch);
                    });
                }
            }

            return batches;
        }

        private async Task<IEnumerable<IBatch>> ReadAllAsync(int syncConfigurationArtifactId, Guid exportRunId)
        {
            ConcurrentBag<IBatch> batches = new ConcurrentBag<IBatch>();
            using (IObjectManager objectManager =
                await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = BatchObjectTypeGuid
                    },
                    Condition = $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId} AND '{_EXPORT_RUN_ID_NAME}' == '{exportRunId}'",
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
                        Batch batch = new Batch(_rdoManager, _serviceFactoryForAdmin, _workspaceArtifactId);
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
            ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, Guid exportRunId,
            int totalDocumentsCount, int startingIndex)
        {
            Batch batch = new Batch(rdoManager, serviceFactory, workspaceArtifactId);
            await batch.CreateAsync(syncConfigurationArtifactId, totalDocumentsCount, startingIndex, exportRunId)
                .ConfigureAwait(false);
            return batch;
        }

        public static async Task<IBatch> GetAsync(IRdoManager rdoManager, ISourceServiceFactoryForAdmin serviceFactoryForAdmin,
            int workspaceArtifactId, int artifactId)
        {
            Batch batch = new Batch(rdoManager, serviceFactoryForAdmin, workspaceArtifactId);
            await batch.InitializeAsync(artifactId).ConfigureAwait(false);
            return batch;
        }

        public static async Task<IEnumerable<IBatch>> GetAllAsync(IRdoManager rdoManager,
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin, int workspaceArtifactId, int syncConfigurationArtifactId, Guid exportRunId)
        {
            Batch batch = new Batch(rdoManager, serviceFactoryForAdmin, workspaceArtifactId);
            IEnumerable<IBatch> batches = await batch.ReadAllAsync(syncConfigurationArtifactId, exportRunId).ConfigureAwait(false);
            return batches;
        }

        public static async Task<IBatch> GetLastAsync(IRdoManager rdoManager,
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin, int workspaceArtifactId, int syncConfigurationArtifactId, Guid exportRunId)
        {
            Batch batch = new Batch(rdoManager, serviceFactoryForAdmin, workspaceArtifactId);
            bool batchFound = await batch.ReadLastAsync(syncConfigurationArtifactId, exportRunId).ConfigureAwait(false);
            return batchFound ? batch : null;
        }

        public static async Task<IEnumerable<int>> GetAllBatchesIdsToExecuteAsync(IRdoManager rdoManager,
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin, int workspaceArtifactId, int syncConfigurationId, Guid exportRunId)
        {
            var batch = new Batch(rdoManager, serviceFactoryForAdmin, workspaceArtifactId);
            IEnumerable<int> batchIds =
                await batch.GetAllBatchesIdsToExecuteAsync(syncConfigurationId, exportRunId).ConfigureAwait(false);
            return batchIds;
        }

        public static async Task<IEnumerable<IBatch>> GetAllSuccessfullyExecutedBatchesAsync(IRdoManager rdoManager,
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin, int workspaceArtifactId, int syncConfigurationId, Guid exportRunId)
        {
            var batch = new Batch(rdoManager, serviceFactoryForAdmin, workspaceArtifactId);
            IEnumerable<IBatch> batches = await batch.GetAllSuccessfullyExecutedBatchesAsync(syncConfigurationId, exportRunId)
                .ConfigureAwait(false);
            return batches;
        }

        public static async Task<IBatch> GetNextAsync(IRdoManager rdoManager,
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin, int workspaceArtifactId, int syncConfigurationArtifactId,
            int startingIndex, Guid exportRunId)
        {
            Batch batch = new Batch(rdoManager, serviceFactoryForAdmin, workspaceArtifactId);
            bool batchFound =
                await batch.ReadNextAsync(syncConfigurationArtifactId, startingIndex, exportRunId).ConfigureAwait(false);
            return batchFound ? batch : null;
        }
    }
}
