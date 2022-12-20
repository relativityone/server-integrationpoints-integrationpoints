using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Services;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Progress;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit
{
    internal class ProgressHandlerTests
    {
        private Mock<ITimerFactory> _timerFactoryFake;
        private Mock<IInstanceSettings> _instanceSettingsFake;
        private Mock<IJobProgressUpdater> _jobProgressUpdaterMock;
        private Mock<IBatchRepository> _batchRepositoryMock;

        private Mock<IImportJobController> _importJobControllerFake;

        private IFixture _fxt;

        private ProgressHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _timerFactoryFake = _fxt.Freeze<Mock<ITimerFactory>>();

            _importJobControllerFake = new Mock<IImportJobController>();

            Mock<ISourceServiceFactoryForAdmin> serviceFactoryAdmin = _fxt.Freeze<Mock<ISourceServiceFactoryForAdmin>>();
            serviceFactoryAdmin.Setup(x => x.CreateProxyAsync<IImportJobController>())
                .ReturnsAsync(_importJobControllerFake.Object);

            _instanceSettingsFake = _fxt.Freeze<Mock<IInstanceSettings>>();
            _jobProgressUpdaterMock = _fxt.Freeze<Mock<IJobProgressUpdater>>();
            _batchRepositoryMock = _fxt.Freeze<Mock<IBatchRepository>>();

            _sut = _fxt.Create<ProgressHandler>();
        }

        [Test]
        public async Task AttachAsync_ShouldReturnEmptyDisposable_WhenExceptionWasThrown()
        {
            // Arrange
            _instanceSettingsFake.Setup(x => x.GetSyncProgressUpdatePeriodAsync(It.IsAny<TimeSpan>()))
                .Throws<Exception>();

            // Act
            IDisposable result = await _sut.AttachAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<int>())
                .ConfigureAwait(false);

            // Assert
            result.Should().NotBeOfType<ITimer>();

            _jobProgressUpdaterMock.Verify(
                x => x.UpdateJobProgressAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Progress.Progress>()),
                Times.Never);
        }

        [Test]
        public async Task AttachAsync_ShouldRunTimerPeriodicallyWithoutDelay()
        {
            // Arrange
            Mock<ITimer> expectedTimer = new Mock<ITimer>();
            TimeSpan expectedPeriod = TimeSpan.FromHours(1);

            _timerFactoryFake.Setup(x => x.Create())
                .Returns(expectedTimer.Object);

            _instanceSettingsFake.Setup(x => x.GetSyncProgressUpdatePeriodAsync(It.IsAny<TimeSpan>()))
                .ReturnsAsync(expectedPeriod);

            // Act
            await _sut.AttachAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<int>())
                .ConfigureAwait(false);

            // Assert
            expectedTimer.Verify(x => x.Activate(It.IsAny<TimerCallback>(), null, TimeSpan.Zero, expectedPeriod));
        }

        [Test]
        public async Task HandleProgressAsync_ShouldUpdateProgressOnJobHistory()
        {
            // Arrange
            ImportProgress progress = _fxt.Create<ImportProgress>();
            ValueResponse<ImportProgress> valueProgress = ValueResponse<ImportProgress>.CreateForSuccess(It.IsAny<Guid>(), progress);
            _importJobControllerFake.Setup(x => x.GetProgressAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(valueProgress);

            List<IBatch> batches = new List<IBatch>(3);
            int completedRecordsCount = progress.ImportedRecords;
            int failedRecordsCount = progress.ErroredRecords;
            foreach (IBatch _ in batches)
            {
                IBatch batch = _fxt.Create<IBatch>();
                completedRecordsCount += batch.ReadDocumentsCount;
                failedRecordsCount += batch.FailedReadDocumentsCount;
                batches.Add(batch);
            }
            _batchRepositoryMock.Setup(x => x.GetBatchesWithIdsAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<Guid>()))
                .ReturnsAsync(batches);

            // Act
            await _sut.HandleProgressAsync().ConfigureAwait(false);

            // Assert
            _jobProgressUpdaterMock.Verify(
                x => x.UpdateJobProgressAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    new Progress.Progress(completedRecordsCount, failedRecordsCount, 0)),
                Times.Once());
        }

        [Test]
        public async Task HandleProgressAsync_ShouldNotThrow_WhenIAPIProgressReadFailed()
        {
            // Arrange
            ValueResponse<ImportProgress> valueProgress = new ValueResponse<ImportProgress>(
                It.IsAny<Guid>(),
                false,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ImportProgress>());

            _importJobControllerFake.Setup(x => x.GetProgressAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(valueProgress);

            // Act
            await _sut.HandleProgressAsync().ConfigureAwait(false);

            // Assert
            _jobProgressUpdaterMock.Verify(
                x => x.UpdateJobProgressAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Progress.Progress>()),
                Times.Never());
        }

        [Test]
        public void HandleProgressAsync_ShouldNotThrow_WhenUpdateProgressOnJobHistoryThrewException()
        {
            // Arrange
            ImportProgress progress = _fxt.Create<ImportProgress>();

            ValueResponse<ImportProgress> valueProgress = ValueResponse<ImportProgress>.CreateForSuccess(It.IsAny<Guid>(), progress);

            _importJobControllerFake.Setup(x => x.GetProgressAsync(It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(valueProgress);

            _jobProgressUpdaterMock.Setup(x =>
                x.UpdateJobProgressAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Progress.Progress>()))
                .Throws<Exception>();

            // Act
            Func<Task> action = async () => await _sut.HandleProgressAsync().ConfigureAwait(false);

            // Assert
            action.Should().NotThrow();
        }
    }
}
