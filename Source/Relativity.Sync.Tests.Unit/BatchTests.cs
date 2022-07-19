using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common.Stubs;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public sealed class BatchTests
    {
        private BatchRepository _batchRepository;
        private Mock<IObjectManager> _objectManager;
        private Mock<IDateTime> _dateTime;
        private FakeRdoManager _fakeRdoManager;

        private const int _WORKSPACE_ID = 433;
        private const int _ARTIFACT_ID = 416;
        private const string _PARENT_OBJECT_FIELD_NAME = "SyncConfiguration";
        internal const string _EXPORT_RUN_ID_NAME = "ExportRunId";

        private static readonly Guid BatchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
        private static readonly Guid StatusGuid = new Guid("D16FAF24-BC87-486C-A0AB-6354F36AF38E");
        private static readonly Guid ExportRunId = new Guid("86A98BD1-7593-46FE-B980-3114CF4D8572");
        
        [SetUp]
        public void SetUp()
        {
            var serviceFactoryForAdminMock = new Mock<ISourceServiceFactoryForAdmin>();
            _dateTime = new Mock<IDateTime>();
            _fakeRdoManager = new FakeRdoManager(_ARTIFACT_ID);
            _batchRepository = new BatchRepository(_fakeRdoManager, serviceFactoryForAdminMock.Object, _dateTime.Object);

            _objectManager = new Mock<IObjectManager>();
            serviceFactoryForAdminMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
            
            _fakeRdoManager.Mock.Setup(x => x.CreateAsync(_WORKSPACE_ID, It.IsAny<SyncBatchRdo>(), It.IsAny<int?>()))
                .Returns((int _, SyncBatchRdo rdo, int? __) =>
                {
                    rdo.ArtifactId = _ARTIFACT_ID;
                    return Task.CompletedTask;
                });
        }

        [Test]
        public async Task CreateAsync_ShouldCreateBatch()
        {
            const int syncConfigurationArtifactId = 634;
            const int totalDocumentsCount = 10000;
            const int startingIndex = 5000;
            BatchStatus defaultStatus = BatchStatus.New;

            

            // ACT
            IBatch batch = await _batchRepository.CreateAsync(_WORKSPACE_ID, syncConfigurationArtifactId, ExportRunId, totalDocumentsCount, startingIndex).ConfigureAwait(false);

            // ASSERT
            batch.TotalDocumentsCount.Should().Be(totalDocumentsCount);
            batch.StartingIndex.Should().Be(startingIndex);
            batch.ArtifactId.Should().Be(_ARTIFACT_ID);
            batch.Status.Should().Be(defaultStatus);

            _fakeRdoManager.Mock.Verify(x => x.CreateAsync(_WORKSPACE_ID, It.IsAny<SyncBatchRdo>(), syncConfigurationArtifactId), Times.Once);
        }

        [Test]
        public async Task GetAsync_ShouldGetBatch()
        {
            const int totalDocumentsCount = 1123;
            const int startingIndex = 532;
            const BatchStatus status = BatchStatus.CompletedWithErrors;
            const int failedItemsCount = 111;
            const int transferredItemsCount = 222;
            const int failedDocumentsCount = 333;
            const int transferredDocumentsCount = 444;
            const int taggedDocumentsCount = 555;
            const int metadataBytesTransferred = 1024;
            const int filesBytesTransferred = 5120;
            const int totalBytesTransferred = 6144;
            
            SyncBatchRdo batchRdo = new SyncBatchRdo
            {
                ArtifactId = _ARTIFACT_ID,
                TotalDocumentsCount = totalDocumentsCount,
                StartingIndex = startingIndex,
                Status = status,
                FailedItemsCount = failedItemsCount,
                TransferredItemsCount = transferredItemsCount,
                TaggedDocumentsCount = taggedDocumentsCount,
                MetadataTransferredBytes = metadataBytesTransferred,
                FilesTransferredBytes = filesBytesTransferred,
                TotalTransferredBytes = totalBytesTransferred,
                FailedDocumentsCount = failedDocumentsCount,
                TransferredDocumentsCount = transferredDocumentsCount
            };

            _fakeRdoManager.Mock.Setup(x => x.GetAsync<SyncBatchRdo>(_WORKSPACE_ID, _ARTIFACT_ID)).ReturnsAsync(batchRdo);
            
            // ACT
            IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

            // ASSERT
            batch.ArtifactId.Should().Be(_ARTIFACT_ID);
            batch.TotalDocumentsCount.Should().Be(totalDocumentsCount);
            batch.StartingIndex.Should().Be(startingIndex);
            batch.Status.Should().Be(status);
            batch.FailedItemsCount.Should().Be(failedItemsCount);
            batch.TransferredItemsCount.Should().Be(transferredItemsCount);
            batch.FailedDocumentsCount.Should().Be(failedDocumentsCount);
            batch.TransferredDocumentsCount.Should().Be(transferredDocumentsCount);
            batch.TaggedDocumentsCount.Should().Be(taggedDocumentsCount);
            batch.MetadataBytesTransferred.Should().Be(metadataBytesTransferred);
            batch.FilesBytesTransferred.Should().Be(filesBytesTransferred);
            batch.TotalBytesTransferred.Should().Be(totalBytesTransferred);

            _fakeRdoManager.Mock.Verify(x => x.GetAsync<SyncBatchRdo>(_WORKSPACE_ID, _ARTIFACT_ID));
        }

        [Test]
        public void GetAsync_ShouldThrow_WhenBatchNotFound()
        {
            _fakeRdoManager.Mock.Setup(x => x.GetAsync<SyncBatchRdo>(_WORKSPACE_ID, _ARTIFACT_ID)).ReturnsAsync((SyncBatchRdo)null);

            // ACT
            Func<Task> action = () => _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID);

            // ASSERT
            action.Should().Throw<SyncException>().Which.Message.Should().Be($"Batch ArtifactID: {_ARTIFACT_ID} not found.");
        }
        
        [Test]
        public async Task SetFailedItemsCountAsync_ShouldUpdateFailedItemsCount()
        {
            const int failedItemsCount = 9876;

            IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

            // ACT
            await batch.SetFailedItemsCountAsync(failedItemsCount).ConfigureAwait(false);

            // ASSERT
            batch.FailedItemsCount.Should().Be(failedItemsCount);

            _fakeRdoManager.Mock.Verify(x => x.SetValueAsync(_WORKSPACE_ID, It.IsAny<SyncBatchRdo>(), v=> v.FailedItemsCount, failedItemsCount));
        }

        [Test]
        public async Task SetTransferredItemsCountAsync_ShouldUpdateTransferredItemsCount()
        {
            const int transferredItemsCount = 849170;
            
            IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

            // ACT
            await batch.SetTransferredItemsCountAsync(transferredItemsCount).ConfigureAwait(false);

            // ASSERT
            batch.TransferredItemsCount.Should().Be(transferredItemsCount);

            _fakeRdoManager.Mock.Verify(x => x.SetValueAsync(_WORKSPACE_ID, It.IsAny<SyncBatchRdo>(), v=> v.TransferredItemsCount, transferredItemsCount));

        }

        [Test]
        public async Task SetMetadataBytesTransferredAsync_ShouldUpdateMetadataBytesTransferred()
        {
            const long metadataBytesTransferred = 1024;

            IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

            // ACT
            await batch.SetMetadataBytesTransferredAsync(metadataBytesTransferred).ConfigureAwait(false);

            // ASSERT
            batch.MetadataBytesTransferred.Should().Be(metadataBytesTransferred);

            _fakeRdoManager.Mock.Verify(x => x.SetValueAsync(_WORKSPACE_ID, It.IsAny<SyncBatchRdo>(), v=> v.MetadataTransferredBytes, metadataBytesTransferred));
        }

        [Test]
        public async Task SetFilesBytesTransferredAsync_ShouldUpdateFilesBytesTransferred()
        {
            const long filesBytesTransferred = 5120;

            IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

            // ACT
            await batch.SetFilesBytesTransferredAsync(filesBytesTransferred).ConfigureAwait(false);

            // ASSERT
            batch.FilesBytesTransferred.Should().Be(filesBytesTransferred);

            _fakeRdoManager.Mock.Verify(x => x.SetValueAsync(_WORKSPACE_ID, It.IsAny<SyncBatchRdo>(), v=> v.FilesTransferredBytes, filesBytesTransferred));
        }

        [Test]
        public async Task SetTotalBytesTransferredAsync_ShouldUpdateTotalBytesTransferred()
        {
            const long totalBytesTransferred = 6144;

            IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

            // ACT
            await batch.SetTotalBytesTransferredAsync(totalBytesTransferred).ConfigureAwait(false);

            // ASSERT
            batch.TotalBytesTransferred.Should().Be(totalBytesTransferred);

            _fakeRdoManager.Mock.Verify(x => x.SetValueAsync(_WORKSPACE_ID, It.IsAny<SyncBatchRdo>(), v=> v.TotalTransferredBytes, totalBytesTransferred));
        }

        [Test]
        public async Task SetFailedDocumentsAsync_ShouldUpdateFailedDocuments()
        {
            const int failedDocumentsCount = 111;

            IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

            // ACT
            await batch.SetFailedDocumentsCountAsync(failedDocumentsCount).ConfigureAwait(false);

            // ASSERT
            batch.FailedDocumentsCount.Should().Be(failedDocumentsCount);

            _fakeRdoManager.Mock.Verify(x => x.SetValueAsync(_WORKSPACE_ID, It.IsAny<SyncBatchRdo>(), v=> v.FailedDocumentsCount, failedDocumentsCount));
        }

        [Test]
        public async Task SetTransferredDocumentsCountAsync_ShouldUpdateTransferredDocumentsCount()
        {
            const int transferredDocumentsCount = 222;

            IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

            // ACT
            await batch.SetTransferredDocumentsCountAsync(transferredDocumentsCount).ConfigureAwait(false);

            // ASSERT
            batch.TransferredDocumentsCount.Should().Be(transferredDocumentsCount);

            _fakeRdoManager.Mock.Verify(x => x.SetValueAsync(_WORKSPACE_ID, It.IsAny<SyncBatchRdo>(), v=> v.TransferredDocumentsCount, transferredDocumentsCount));
        }

        [Test]
        public async Task SetStatusAsync_ShouldUpdateStatus()
        {
            const BatchStatus status = BatchStatus.InProgress;
            IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

            // ACT
            await batch.SetStatusAsync(status).ConfigureAwait(false);

            // ASSERT
            batch.Status.Should().Be(status);

            _fakeRdoManager.Mock.Verify(x => x.SetValueAsync(_WORKSPACE_ID, It.IsAny<SyncBatchRdo>(), v=> v.Status, status));
        }

        [Test]
        public async Task SetTaggedDocumentsCountAsync_ShouldUpdateTaggedDocumentsCount()
        {
            const int taggedDocumentsCount = 849170;

            IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

            // ACT
            await batch.SetTaggedDocumentsCountAsync(taggedDocumentsCount).ConfigureAwait(false);

            // ASSERT
            batch.TaggedDocumentsCount.Should().Be(taggedDocumentsCount);

            _fakeRdoManager.Mock.Verify(x => x.SetValueAsync(_WORKSPACE_ID, It.IsAny<SyncBatchRdo>(), v=> v.TaggedDocumentsCount, taggedDocumentsCount));
        }

        [Test]
        public async Task GetLastAsync_ShouldReturnNull_WhenNoBatchesFound()
        {
            const int syncConfigurationArtifactId = 845967;

            QueryResult queryResult = new QueryResult();
            _objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(queryResult);

            // ACT
            IBatch batch = await _batchRepository.GetLastAsync(_WORKSPACE_ID, syncConfigurationArtifactId, ExportRunId).ConfigureAwait(false);

            // ASSERT
            batch.Should().BeNull();
        }

        [Test]
        public async Task GetLastAsync_ShouldReturnLastBatch()
        {
            const int syncConfigurationArtifactId = 845967;

            QueryResult queryResult = new QueryResult
            {
                Objects = new List<RelativityObject> { new RelativityObject() }
            };
            queryResult.TotalCount = 1;

            _objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(queryResult);

            // ACT
            IBatch batch = await _batchRepository.GetLastAsync(_WORKSPACE_ID, syncConfigurationArtifactId, ExportRunId).ConfigureAwait(false);

            // ASSERT
            batch.Should().NotBeNull();
            _objectManager.Verify(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequest(qr, syncConfigurationArtifactId, ExportRunId)), 1, 1), Times.Once);
        }

        private bool AssertQueryRequest(QueryRequest queryRequest, int syncConfigurationArtifactId, Guid exportRunId)
        {
            queryRequest.ObjectType.Guid.Should().Be(BatchObjectTypeGuid);
            queryRequest.Condition.Should().Be($"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId} AND '{_EXPORT_RUN_ID_NAME}' == '{exportRunId}'");
            return true;
        }

        private bool AssertQueryRequestSlim(QueryRequest queryRequest, int syncConfigurationArtifactId, Guid exportRunId)
        {
            queryRequest.ObjectType.Guid.Should().Be(BatchObjectTypeGuid);
            queryRequest.Condition.Should().Be($"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId} AND '{_EXPORT_RUN_ID_NAME}' == '{exportRunId}'");
            return true;
        }

        [Test]
        public async Task GetAllBatchesIdsToExecuteAsync_ShouldReturnPausedAndThenNewBatchIds()
        {
            // Arrange
            _objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(r => r.Condition.Contains("New")), 1, int.MaxValue)).ReturnsAsync(PrepareQueryResult(artifactId: 1));
            _objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(r => r.Condition.Contains("Paused")), 1, int.MaxValue)).ReturnsAsync(PrepareQueryResult(artifactId: 2));
            
            

            // Act
            IEnumerable<int> batchIds = await _batchRepository.GetAllBatchesIdsToExecuteAsync(_WORKSPACE_ID, _ARTIFACT_ID, ExportRunId).ConfigureAwait(false);

            // Assert
            batchIds.Should().NotBeNullOrEmpty();
            batchIds.Should().ContainInOrder(new[] {2, 1});

            VerifyQueryAllRequests();
        }

        [Test]
        public async Task GetAllNewBatchesIdsAsync_ShouldReturnNoBatchIds_WhenNoNewBatchesExist()
        {
            // Arrange
            var queryResult = new QueryResult();
            _objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, int.MaxValue)).ReturnsAsync(queryResult);

            // Act
            IEnumerable<int> batchIds = await _batchRepository.GetAllBatchesIdsToExecuteAsync(_WORKSPACE_ID, _ARTIFACT_ID, ExportRunId).ConfigureAwait(false);

            // Assert
            batchIds.Should().NotBeNull();
            batchIds.Should().BeEmpty();
            batchIds.Any().Should().BeFalse();

            VerifyQueryAllRequests();
        }

        [Test]
        public void GetAllNewBatchesIdsAsync_ShouldThrow_WhenItFailsToQueryForNewBatches()
        {
            // Arrange
            _objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, int.MaxValue)).Throws<NotAuthorizedException>();

            // Act & Assert
            Assert.ThrowsAsync<NotAuthorizedException>(() => _batchRepository.GetAllBatchesIdsToExecuteAsync(_WORKSPACE_ID, _ARTIFACT_ID, ExportRunId));

            VerifyQueryAllRequests();
        }

        [Test]
        public async Task GetAllAsync_ShouldReadAllBatches()
        {
            const int syncConfigurationArtifactId = 634;

            QueryResultSlim queryResultSlim = PrepareQueryResultSlim();
            queryResultSlim.TotalCount = queryResultSlim.Objects.Count;
            _objectManager.Setup(x => x.QuerySlimAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, int.MaxValue)).ReturnsAsync(queryResultSlim);

            QueryResult queryResult = PrepareQueryResult();
            _objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

            // ACT
            IEnumerable<IBatch> batches = await _batchRepository.GetAllAsync(_WORKSPACE_ID, syncConfigurationArtifactId, ExportRunId).ConfigureAwait(false);

            // ASSERT
            batches.Should().NotBeNullOrEmpty();
            batches.Should().NotContainNulls();

            _objectManager.Verify(x => x.QuerySlimAsync(_WORKSPACE_ID, It.Is<QueryRequest>(rr => AssertQueryRequestSlim(rr, syncConfigurationArtifactId, ExportRunId)), 1, int.MaxValue), Times.Once);
        }

        [Test]
        public async Task DeleteAllForConfiguration_ShouldDeleteAllBatchesForGivenConfiguration()
        {
            const int syncConfigurationArtifactId = 634;

            // ACT
            await _batchRepository.DeleteAllForConfigurationAsync(_WORKSPACE_ID, syncConfigurationArtifactId).ConfigureAwait(false);

            // ASSERT
            _objectManager.Verify(x => x.DeleteAsync(_WORKSPACE_ID, It.Is<MassDeleteByCriteriaRequest>(request => AssertMassDeleteByCriteriaRequest(request, syncConfigurationArtifactId))));
        }

        private bool AssertMassDeleteByCriteriaRequest(MassDeleteByCriteriaRequest request, int syncConfigurationArtifactId)
        {
            return
                request.ObjectIdentificationCriteria.ObjectType.Guid == BatchObjectTypeGuid &&
                request.ObjectIdentificationCriteria.Condition == $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}";
        }
        
        private void VerifyQueryAllRequests()
        {
            void VerifyStatusWasRead(BatchStatus status)
            {
                string expectedCondition =
                    $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {_ARTIFACT_ID} AND '{StatusGuid}' == '{status.GetDescription()}' AND '{_EXPORT_RUN_ID_NAME}' == '{ExportRunId}'";
                _objectManager.Verify(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(rr => rr.ObjectType.Guid == BatchObjectTypeGuid && rr.Condition == expectedCondition), 1, int.MaxValue), Times.Once);
            }
            
            VerifyStatusWasRead(BatchStatus.Paused);
            VerifyStatusWasRead(BatchStatus.New);
        }
        
        private static QueryResult PrepareQueryResult( int? artifactId = null)
        {
            QueryResult readResult = new QueryResult
            {
                Objects = new List<RelativityObject>()
                {
                    new RelativityObject
                    {
                        ArtifactID = artifactId ?? _ARTIFACT_ID
                    }
                }
            };

            readResult.TotalCount = readResult.Objects.Count();
            return readResult;
        }
        private static QueryResultSlim PrepareQueryResultSlim()
        {
            return new QueryResultSlim
            {
                Objects = new List<RelativityObjectSlim>
                {
                    PrepareObjectSlim(),
                    PrepareObjectSlim()
                },
                TotalCount = 2
            };
        }

        private static RelativityObjectSlim PrepareObjectSlim()
        {
            return new RelativityObjectSlim
            {
                ArtifactID = _ARTIFACT_ID
            };
        }
    }
}