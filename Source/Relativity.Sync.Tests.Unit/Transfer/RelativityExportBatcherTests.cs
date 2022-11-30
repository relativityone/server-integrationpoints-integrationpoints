using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
    [TestFixture]
    internal sealed class RelativityExportBatcherTests
    {
        private Mock<IObjectManager> _objectManagerMock;
        private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUserStub;

        [SetUp]
        public void SetUp()
        {
            _objectManagerMock = new Mock<IObjectManager>();

            _serviceFactoryForUserStub = new Mock<ISourceServiceFactoryForUser>();
            _serviceFactoryForUserStub.Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManagerMock.Object);
        }

        [Test]
        public async Task GetNextItemsFromBatchAsync_ShouldRespectAlreadyTransferredItems_AndExportRunId_WhenSetRemainingItems()
        {
            // Arrange
            const int totalDocumentsCount = 10;
            const int transferredDocumentsCount = 3;
            Guid exportRunId = Guid.NewGuid();

            int expectedRemainingItemsCount = totalDocumentsCount - transferredDocumentsCount;

            Mock<IBatch> batch = new Mock<IBatch>();
            batch.SetupGet(x => x.TotalDocumentsCount).Returns(totalDocumentsCount);
            batch.SetupGet(x => x.TransferredDocumentsCount).Returns(transferredDocumentsCount);
            batch.SetupGet(x => x.ExportRunId).Returns(Guid.Empty);
            batch.SetupGet(x => x.ExportRunId).Returns(exportRunId);


            RelativityExportBatcher sut = new RelativityExportBatcher(_serviceFactoryForUserStub.Object, batch.Object, It.IsAny<int>());

            // Act
            await sut.GetNextItemsFromBatchAsync().ConfigureAwait(false);

            // Assert
            _objectManagerMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(
                It.IsAny<int>(), exportRunId, expectedRemainingItemsCount, It.IsAny<int>()));
        }

        [Test]
        public void GetNextItemsFromBatchAsync_ShouldReturnAllItemsInOneBlock()
        {
            // arrange
            Mock<IBatch> batch = new Mock<IBatch>();
            batch.SetupGet(x => x.StartingIndex).Returns(0);
            const int totalItemsCount = 10;
            batch.SetupGet(x => x.TotalDocumentsCount).Returns(totalItemsCount);
            SetupRetrieveResultsBlock(totalItemsCount);
            RelativityExportBatcher exportBatcher = new RelativityExportBatcher(_serviceFactoryForUserStub.Object, batch.Object, 0);

            // act
            Task<RelativityObjectSlim[]> firstResultsBlock = exportBatcher.GetNextItemsFromBatchAsync();
            Task<RelativityObjectSlim[]> secondResultsBlock = exportBatcher.GetNextItemsFromBatchAsync();

            // assert
            firstResultsBlock.Result.Length.Should().Be(totalItemsCount);
            secondResultsBlock.Result.Length.Should().Be(0);
        }

        [Test]
        public void GetNextItemsFromBatchAsync_ShouldReturnItemsInTwoBlocks()
        {
            // arrange
            Mock<IBatch> batchStub = new Mock<IBatch>();
            batchStub.SetupGet(x => x.StartingIndex).Returns(0);
            const int totalItemsCount = 10;
            const int maxResultsBlockSize = 7;
            batchStub.SetupGet(x => x.TotalDocumentsCount).Returns(totalItemsCount);
            RelativityExportBatcher exportBatcher = new RelativityExportBatcher(_serviceFactoryForUserStub.Object, batchStub.Object, 0);

            // act
            SetupRetrieveResultsBlock(maxResultsBlockSize);
            Task<RelativityObjectSlim[]> firstResultsBlock = exportBatcher.GetNextItemsFromBatchAsync();
            SetupRetrieveResultsBlock(totalItemsCount - maxResultsBlockSize);
            Task<RelativityObjectSlim[]> secondResultsBlock = exportBatcher.GetNextItemsFromBatchAsync();

            // assert
            firstResultsBlock.Result.Length.Should().Be(maxResultsBlockSize);
            secondResultsBlock.Result.Length.Should().Be(totalItemsCount - maxResultsBlockSize);
        }


        [Test]
        public async Task GetNextItemsFromBatchAsync_ShouldNotCallObjectManagerWhenRemainingItemsIsZero()
        {
            // arrange
            const int totalItemsCount = 10;

            SetupRetrieveResultsBlock(totalItemsCount);

            Mock<IBatch> batchStub = new Mock<IBatch>();
            batchStub.SetupGet(x => x.StartingIndex).Returns(0);
            batchStub.SetupGet(x => x.TotalDocumentsCount).Returns(0);

            RelativityExportBatcher batcher = new RelativityExportBatcher(_serviceFactoryForUserStub.Object, batchStub.Object, 0);

            // act
            RelativityObjectSlim[] batches = await batcher.GetNextItemsFromBatchAsync().ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(0, Guid.Empty, 0, It.IsAny<int>()), Times.Never);
            batches.Any().Should().BeFalse();
        }

        private void SetupRetrieveResultsBlock(int maxResultSize)
        {
            _objectManagerMock.Setup(x =>
                x.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())
            ).ReturnsAsync<int, Guid, int, int, IObjectManager, RelativityObjectSlim[]>((a, b, len, ind) =>
                CreateBatch(ind, len).Take(maxResultSize).ToArray());
        }

        private static IEnumerable<RelativityObjectSlim> CreateBatch(int startingIndex, int length)
        {
            return Enumerable.Range(startingIndex, length).Select(x => new RelativityObjectSlim { ArtifactID = x });
        }
    }
}
