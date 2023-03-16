using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Common.Stubs;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal class JobStatusConsolidationExecutorTests
    {
        private Mock<IObjectManager> _objectManagerFake;
        private Mock<IBatchRepository> _batchRepositoryStub;
        private Mock<IJobStatisticsContainer> _jobStatisticsContainerStub;
        private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryForAdminStub;
        private Mock<IJobStatusConsolidationConfiguration> _configurationStub;
        private Mock<IIAPIv2RunChecker> _iapiv2CheckFake;

        private IFixture _fxt;

        private List<IBatch> _batches;

        private IExecutor<IJobStatusConsolidationConfiguration> _sut;

        private static readonly InvalidOperationException _EXCEPTION = new InvalidOperationException();
        private static readonly Guid _COMPLETED_ITEMS_COUNT_GUID = new Guid("70680399-c8ea-4b12-b711-e9ecbc53cb1c");
        private static readonly Guid _FAILED_ITEMS_COUNT_GUID = new Guid("c224104f-c1ca-4caa-9189-657e01d5504e");
        private static readonly Guid _TOTAL_ITEMS_COUNT_GUID = new Guid("576189a9-0347-4b20-9369-b16d1ac89b4b");

        public static IEnumerable<Action<JobStatusConsolidationExecutorTests>> ServiceFailures { get; } = new Action<JobStatusConsolidationExecutorTests>[]
        {
            ctx => ctx._serviceFactoryForAdminStub
                .Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ThrowsAsync(_EXCEPTION),
            ctx => ctx._objectManagerFake
                .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()))
                .ThrowsAsync(_EXCEPTION),
            ctx => ctx._batchRepositoryStub
                .Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .ThrowsAsync(_EXCEPTION)
        };

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _objectManagerFake = new Mock<IObjectManager>();
            _batchRepositoryStub = new Mock<IBatchRepository>();
            _jobStatisticsContainerStub = new Mock<IJobStatisticsContainer>();
            _configurationStub = new Mock<IJobStatusConsolidationConfiguration>();
            _configurationStub.Setup(x => x.ExportRunId).Returns(new Guid("286E0000-479B-4752-B95A-C818A3974495"));

            _serviceFactoryForAdminStub = new Mock<ISourceServiceFactoryForAdmin>();
            _serviceFactoryForAdminStub
                .Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManagerFake.Object);

            _iapiv2CheckFake = new Mock<IIAPIv2RunChecker>();
            _iapiv2CheckFake.Setup(x => x.ShouldBeUsed()).Returns(false);

            _batches = new List<IBatch>();

            _batchRepositoryStub
                .Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(() => _batches);

            Mock<IAPILog> log = new Mock<IAPILog>();

            SetUpUpdateCall(success: true);

            _sut = new JobStatusConsolidationExecutor(
                new ConfigurationStub(),
                _batchRepositoryStub.Object,
                _jobStatisticsContainerStub.Object,
                _serviceFactoryForAdminStub.Object,
                _iapiv2CheckFake.Object,
                log.Object);
        }

        [Test]
        [TestCaseSource(nameof(ServiceFailures))]
        public async Task ExecuteAsync_ShouldReportFailure_WhenAnyServiceThrow(Action<JobStatusConsolidationExecutorTests> setUpFailure)
        {
            // Arrange
            const int totalBatchCount = 5;
            const int totalTransferredCount = 1000;
            const int totalFailedCount = 10;
            SetUpBatches(totalBatchCount, totalTransferredCount, totalFailedCount);

            setUpFailure(this);

            // Act
            ExecutionResult result = await _sut
                .ExecuteAsync(_configurationStub.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Exception.Should().Be(_EXCEPTION);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReportFailure_WhenFailedToUpdateJobHistory()
        {
            // Arrange
            const int totalBatchCount = 5;
            const int totalTransferredCount = 1000;
            const int totalFailedCount = 10;
            SetUpBatches(totalBatchCount, totalTransferredCount, totalFailedCount);

            SetUpUpdateCall(success: false);

            // Act
            ExecutionResult result = await _sut
                .ExecuteAsync(_configurationStub.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Exception.Should().Be(null);
        }

        [Test]
        public async Task ExecuteAsync_ShouldAggregateStatisticsFromBatches_WhenNoImagesStatisticsPresent(
            [Values(1, 2, 5, 8)] int batchCount,
            [Values(0, 2, 10, 1000)] int transferredCount,
            [Values(0, 2, 10, 1000)] int failedCount)
        {
            // Arrange
            int totalItemCount = transferredCount + failedCount;

            _jobStatisticsContainerStub.SetupGet(p => p.ImagesStatistics).Returns(() => null);
            SetUpBatches(batchCount, transferredCount, failedCount);

            // Act
            ExecutionResult result = await _sut
                .ExecuteAsync(_configurationStub.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            VerifyUpdateCall(transferredCount, failedCount, totalItemCount);
        }

        [Test]
        public async Task ExecuteAsync_ShouldAggregateStatisticsFromBatches_WhenNewImportFlowIsUsed()
        {
            // Arrange
            _batches = _fxt.CreateMany<BatchStub>().ToList<IBatch>();

            int expectedTransferred = _batches.Sum(x => x.TransferredDocumentsCount);
            int expectedFailed = _batches.Sum(x => x.FailedDocumentsCount);
            int expectedTotal = _batches.Sum(x => x.TotalDocumentsCount);

            _iapiv2CheckFake.Setup(x => x.ShouldBeUsed()).Returns(true);

            // Act
            ExecutionResult result = await _sut
                .ExecuteAsync(_configurationStub.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            VerifyUpdateCall(
                expectedTransferred,
                expectedFailed,
                expectedTotal);
        }

        [Test]
        public async Task ExecuteAsync_ShouldAggregateStatisticsFromBatchesAndStatistics_WhenImagesStatisticsPresent(
            [Values(1, 2, 5, 8)] int batchCount,
            [Values(0, 2, 10, 1000)] int transferredCount,
            [Values(0, 2, 10, 1000)] int failedCount)
        {
            // Arrange
            int totalItemCount = transferredCount + failedCount;
            _jobStatisticsContainerStub.SetupGet(p => p.ImagesStatistics).Returns(Task.FromResult(new ImagesStatistics(totalItemCount, 0)));
            SetUpBatches(batchCount, transferredCount, failedCount, 0);

            // Act
            ExecutionResult result = await _sut
                .ExecuteAsync(_configurationStub.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            VerifyUpdateCall(transferredCount, failedCount, totalItemCount);
        }

        [Test]
        public async Task ExecuteAsync_ShouldUpdateWithZeros_WhenThereAreNoBatches()
        {
            // Act
            ExecutionResult result = await _sut
                .ExecuteAsync(_configurationStub.Object, CompositeCancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(ExecutionStatus.Completed);
            VerifyUpdateCall(transferredCount: 0, failedCount: 0, totalItemCount: 0);
        }

        private void SetUpUpdateCall(bool success)
        {
            _objectManagerFake
                .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()))
                .ReturnsAsync(() => CreateUpdateResultStatuses(success));
        }

        private static UpdateResult CreateUpdateResultStatuses(bool success)
        {
            var updateResult = new UpdateResult
            {
                EventHandlerStatuses = new List<EventHandlerStatus>
                {
                    new EventHandlerStatus { Success = success }
                }
            };
            return updateResult;
        }

        private void SetUpBatches(int batchesTotalCount, int totalTransferred, int totalFailed, int? batchItemsCount = null)
        {
            if (batchesTotalCount > 0)
            {
                _batches.AddRange(CreateBatches(batchesTotalCount, totalTransferred, totalFailed, batchItemsCount));
            }
        }

        private static IEnumerable<IBatch> CreateBatches(int count, int transferred, int failed, int? batchItemsCount = null)
        {
            int total = transferred + failed;
            int batchSize = total / count;

            IEnumerable<IBatch> batches = Enumerable
                .Range(0, total)
                .Select(i => i < transferred)
                .ToList()
                .Shuffle()
                .SplitList(batchSize)
                .Select(isTransferredList =>
                    CreateBatch(isTransferredList.Count(t => t), isTransferredList.Count(t => !t), batchItemsCount));

            return batches;
        }

        private static IBatch CreateBatch(int transferred, int failed, int? batchItemsCount = null)
        {
            var batch = new Mock<IBatch>();

            batch
                .SetupGet(b => b.TransferredItemsCount)
                .Returns(transferred);

            batch
                .SetupGet(b => b.FailedItemsCount)
                .Returns(failed);

            batch
                .SetupGet(b => b.TotalDocumentsCount)
                .Returns(batchItemsCount ?? transferred + failed);

            return batch.Object;
        }

        private void VerifyUpdateCall(int transferredCount, int failedCount, int totalItemCount)
        {
            _objectManagerFake
                .Verify(
                    x => x.UpdateAsync(It.IsAny<int>(), It.Is<UpdateRequest>(r =>
                    (int)r.FieldValues.Single(fvp => fvp.Field.Guid.Equals(_COMPLETED_ITEMS_COUNT_GUID)).Value == transferredCount &&
                    (int)r.FieldValues.Single(fvp => fvp.Field.Guid.Equals(_FAILED_ITEMS_COUNT_GUID)).Value == failedCount &&
                    (int)r.FieldValues.Single(fvp => fvp.Field.Guid.Equals(_TOTAL_ITEMS_COUNT_GUID)).Value == totalItemCount)),
                    Times.Once);
        }
    }
}
