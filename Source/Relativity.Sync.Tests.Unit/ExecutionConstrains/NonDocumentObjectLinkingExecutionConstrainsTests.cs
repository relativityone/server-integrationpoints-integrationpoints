using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    [TestFixture]
    public class NonDocumentObjectLinkingExecutionConstrainsTests
    {
        [TestCase(true, true)]
        [TestCase(false, false)]
        public async Task CanExecute_ShouldReturnCorrectValue(bool linkingExportExists, bool expectedResult)
        {
            // Arrange
            var configMock = new Mock<INonDocumentObjectLinkingConfiguration>();
            Guid exportRunId = Guid.NewGuid();
            configMock.SetupGet(x => x.ObjectLinkingSnapshotId)
                .Returns(linkingExportExists ? (Guid?)exportRunId : null);

            var batchRepositoryMock = new Mock<IBatchRepository>();
            batchRepositoryMock.Setup(x =>
                    x.GetAllBatchesIdsToExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), exportRunId))
                .ReturnsAsync(linkingExportExists ? new[] { 1 } : Array.Empty<int>());

            var sut = new NonDocumentObjectLinkingExecutionConstrains(batchRepositoryMock.Object, new Mock<IAPILog>().Object);

            // Act
            bool result = await sut.CanExecuteAsync(configMock.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Test]
        public async Task CanExecute_ShouldLogSkippingLinking()
        {
            // Arrange
            Mock<INonDocumentObjectLinkingConfiguration> configMock = new Mock<INonDocumentObjectLinkingConfiguration>();
            configMock.SetupGet(x => x.ObjectLinkingSnapshotId)
                .Returns((Guid?)null);

            Mock<IBatchRepository> batchRepositoryMock = new Mock<IBatchRepository>();

            Mock<IAPILog> logMock = new Mock<IAPILog>();
            NonDocumentObjectLinkingExecutionConstrains sut = new NonDocumentObjectLinkingExecutionConstrains(batchRepositoryMock.Object, logMock.Object);

            // Act
            bool result = await sut.CanExecuteAsync(configMock.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().Be(false);
            logMock.Verify(x => x.LogInformation(
                $"{nameof(INonDocumentObjectLinkingConfiguration.ObjectLinkingSnapshotId)} is empty - skipping object linking"));
        }
    }
}
