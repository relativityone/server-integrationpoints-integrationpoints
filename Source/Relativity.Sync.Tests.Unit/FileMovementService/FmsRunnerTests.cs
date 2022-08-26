using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.FileMovementService;
using Relativity.Sync.Transfer.FileMovementService.Models;

namespace Relativity.Sync.Tests.Unit.FileMovementService
{
    [TestFixture]
    public class FmsRunnerTests
    {
        private Mock<IFmsClient> _fmsClientMock;
        private FmsRunner _sut;

        [SetUp]
        public void SetUp()
        {
            Mock<IFmsInstanceSettingsService> fmsInstanceSettingsMock = new Mock<IFmsInstanceSettingsService>();
            fmsInstanceSettingsMock.Setup(x => x.GetMonitoringInterval()).ReturnsAsync(0); // no delay between calls

            _fmsClientMock = new Mock<IFmsClient>();
            _fmsClientMock.Setup(x => x.CopyListOfFilesAsync(It.IsAny<CopyListOfFilesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CopyListOfFilesResponse());

            _sut = new FmsRunner(_fmsClientMock.Object, fmsInstanceSettingsMock.Object,  new EmptyLogger());
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public async Task RunAsync_ShouldReturnStatusInfoList(int batchesCount)
        {
            // Arrange
            Dictionary<int, NativeFilePathStructure> filePaths = new Dictionary<int, NativeFilePathStructure>
            {
                { 1, new NativeFilePathStructure($@"/path/1/2/3/4/1/") },
            };
            List<FmsBatchInfo> input = Enumerable.Range(0, batchesCount)
                .Select(i => new FmsBatchInfo(1, filePaths, String.Empty, Guid.Empty))
                .ToList();

            // Act
            List<FmsBatchStatusInfo> result = await _sut.RunAsync(input, CancellationToken.None);

            // Assert
            result.Count.Should().Be(batchesCount);
            _fmsClientMock.Verify(m => m.CopyListOfFilesAsync(It.IsAny<CopyListOfFilesRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(batchesCount));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public async Task MonitorAsync_ForSucceededBatches_ShouldCallGetRunStatusOncePerBatch(int batchesCount)
        {
            // Arrange
            _fmsClientMock.Setup(x => x.GetRunStatusAsync(It.IsAny<RunStatusRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RunStatusResponse { Status = RunStatuses.Succeeded });

            List<FmsBatchStatusInfo> input = Enumerable.Range(0, batchesCount)
                .Select(i => new FmsBatchStatusInfo { Status = RunStatuses.InProgress })
                .ToList();

            // Act
            await _sut.MonitorAsync(input, CancellationToken.None);

            // Assert
            _fmsClientMock.Verify(m => m.GetRunStatusAsync(It.IsAny<RunStatusRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(batchesCount));
        }

        [TestCase(FmsRunner.SubmittedStatusName, 1)]
        [TestCase(RunStatuses.Queued, 1)]
        [TestCase(RunStatuses.InProgress, 1)]
        [TestCase(RunStatuses.Canceling, 1)]
        [TestCase(RunStatuses.Succeeded, 0)]
        [TestCase(RunStatuses.Failed, 0)]
        [TestCase(RunStatuses.Cancelled, 0)]
        public async Task MonitorAsync_DependOnInitialStatus_ShouldCallGetRunStatusOrNot(string initialStatus, int expectedCalls)
        {
            // Arrange
            _fmsClientMock.Setup(x => x.GetRunStatusAsync(It.IsAny<RunStatusRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RunStatusResponse { Status = RunStatuses.Succeeded });

            List<FmsBatchStatusInfo> input = Enumerable.Range(0, 1)
                .Select(i => new FmsBatchStatusInfo { Status = initialStatus })
                .ToList();

            // Act
            await _sut.MonitorAsync(input, CancellationToken.None);

            // Assert
            _fmsClientMock.Verify(m => m.GetRunStatusAsync(It.IsAny<RunStatusRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(expectedCalls));
        }



        [Test]
        public async Task MonitorAsync_ShouldCallGetRunStatusUntilSucceeded()
        {
            // Arrange
            _fmsClientMock.SetupSequence(x => x.GetRunStatusAsync(It.IsAny<RunStatusRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RunStatusResponse { Status = RunStatuses.Queued })
                .ReturnsAsync(new RunStatusResponse { Status = RunStatuses.InProgress })
                .ReturnsAsync(new RunStatusResponse { Status = RunStatuses.Succeeded });

            List<FmsBatchStatusInfo> input = new List<FmsBatchStatusInfo> { new FmsBatchStatusInfo { Status = RunStatuses.InProgress } };

            // Act
            await _sut.MonitorAsync(input, CancellationToken.None);

            // Assert
            _fmsClientMock.Verify(m => m.GetRunStatusAsync(It.IsAny<RunStatusRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }
    }
}
