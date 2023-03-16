using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Sync.Executors;
using Relativity.Sync.Progress;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Common.Stubs;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit
{
    internal class ProgressHandlerTests
    {
        private Mock<ITimerFactory> _timerFactoryFake;
        private Mock<IInstanceSettings> _instanceSettingsFake;
        private Mock<IJobProgressUpdater> _jobProgressUpdaterMock;
        private Mock<IBatchRepository> _batchRepositoryMock;
        private Mock<IImportService> _importServiceMock;

        private IFixture _fxt;

        private ProgressHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _timerFactoryFake = _fxt.Freeze<Mock<ITimerFactory>>();

            _instanceSettingsFake = _fxt.Freeze<Mock<IInstanceSettings>>();
            _jobProgressUpdaterMock = _fxt.Freeze<Mock<IJobProgressUpdater>>();

            _batchRepositoryMock = _fxt.Freeze<Mock<IBatchRepository>>();
            _importServiceMock = _fxt.Freeze<Mock<IImportService>>();

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
                    It.IsAny<int>(),
                    It.IsAny<IEnumerable<int>>())
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
                    _fxt.Create<int>(),
                    _fxt.Create<int>(),
                    _fxt.Create<int>(),
                    _fxt.Create<Guid>(),
                    _fxt.Create<int>(),
                    _fxt.Create<IEnumerable<int>>())
                .ConfigureAwait(false);

            // Assert
            expectedTimer.Verify(x => x.Activate(It.IsAny<TimerCallback>(), null, TimeSpan.Zero, expectedPeriod));
        }

        [Test]
        public async Task HandleProgressAsync_ShouldUpdateProgressOnJobHistory()
        {
            // Arrange
            ImportProgress importProgress = _fxt.Create<ImportProgress>();
            ValueResponse<ImportProgress> valueProgress = ValueResponse<ImportProgress>.CreateForSuccess(It.IsAny<Guid>(), importProgress);
            _importServiceMock.Setup(x => x.GetJobImportProgressValueAsync())
                 .ReturnsAsync(valueProgress.Value);

            var importJobProgress = new Progress.Progress(
                0,
                importProgress.ErroredRecords,
                importProgress.ImportedRecords);

            IEnumerable<IBatch> batches = _fxt.CreateMany<BatchStub>().ToList<IBatch>();

            _batchRepositoryMock.Setup(x =>
                    x.GetBatchesWithIdsAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<List<int>>(),
                        It.IsAny<Guid>()))
                .ReturnsAsync(batches);

            var batchProgress = new Progress.Progress(
                batches.Sum(x => x.ReadDocumentsCount),
                batches.Sum(x => x.FailedReadDocumentsCount),
                0);

            Progress.Progress expectedProgress = batchProgress + importJobProgress;

            // Act
            await _sut.HandleProgressAsync().ConfigureAwait(false);

            // Assert
            _jobProgressUpdaterMock.Verify(
                x => x.UpdateJobProgressAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.Is<Progress.Progress>(p => p.Equals(expectedProgress))),
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

            _importServiceMock.Setup(x => x.GetJobImportProgressValueAsync())
                  .ReturnsAsync(valueProgress.Value);

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
            ImportProgress importProgress = _fxt.Create<ImportProgress>();

            ValueResponse<ImportProgress> valueProgress = ValueResponse<ImportProgress>.CreateForSuccess(It.IsAny<Guid>(), importProgress);

            _importServiceMock.Setup(x => x.GetJobImportProgressValueAsync())
                  .ReturnsAsync(valueProgress.Value);

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

        [Test]
        public async Task HandleProgressAsync_ShouldRunOnlyOneThreadAtOnce()
        {
            // Arrange
            ImportProgress importProgress = _fxt.Create<ImportProgress>();
            ValueResponse<ImportProgress> valueProgress = ValueResponse<ImportProgress>.CreateForSuccess(It.IsAny<Guid>(), importProgress);
            _importServiceMock
                .Setup(x => x.GetJobImportProgressValueAsync())
                .ReturnsAsync(() =>
                {
                    Task.Delay(500).GetAwaiter().GetResult();
                    return valueProgress.Value;
                });

            IEnumerable<IBatch> batches = _fxt.Build<BatchStub>()
                .With(x => x.Status, BatchStatus.InProgress)
                .CreateMany()
                .ToList<IBatch>();

            _batchRepositoryMock.Setup(x =>
                    x.GetBatchesWithIdsAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<List<int>>(),
                        It.IsAny<Guid>()))
                .ReturnsAsync(batches);

            // Act
            Task progress1 = Task.Run(() => _sut.HandleProgressAsync());

            Task progress2 = Task.Run(() => _sut.HandleProgressAsync());

            await Task.WhenAll(progress1, progress2).ConfigureAwait(false);

            // Assert
            _jobProgressUpdaterMock.Verify(
                x => x.UpdateJobProgressAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Progress.Progress>()),
                Times.Once());
        }

        [Test]
        public async Task HandleProgressAsync_ShouldAddToCacheOnlyFinishedBatches()
        {
            // Arrange
            ImportProgress importProgress = _fxt.Create<ImportProgress>();
            ValueResponse<ImportProgress> valueProgress = ValueResponse<ImportProgress>.CreateForSuccess(It.IsAny<Guid>(), importProgress);
            _importServiceMock.Setup(x => x.GetJobImportProgressValueAsync())
                  .ReturnsAsync(valueProgress.Value);

            IEnumerable<IBatch> batches = _fxt.Build<BatchStub>()
                .With(x => x.Status, () => BatchStatus.Generated)
                .CreateMany()
                .ToList<IBatch>();

            _batchRepositoryMock.Setup(x =>
                    x.GetBatchesWithIdsAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<List<int>>(),
                        It.IsAny<Guid>()))
                .ReturnsAsync(batches);

            // Act
            await _sut.AttachAsync(
                    _fxt.Create<int>(),
                    _fxt.Create<int>(),
                    _fxt.Create<int>(),
                    _fxt.Create<Guid>(),
                    _fxt.Create<int>(),
                    batches.Select(x => x.ArtifactId))
                .ConfigureAwait(false);

            await _sut.HandleProgressAsync().ConfigureAwait(false);

            // Assert
            var readDocumentsCountCache = _sut?
                .GetType()?
                .GetField("_readDocumentsCountCache", BindingFlags.Instance | BindingFlags.NonPublic)?
                .GetValue(_sut);

            var failedReadDocumentsCountCache = _sut?
                .GetType()?
                .GetField("_failedReadDocumentsCountCache", BindingFlags.Instance | BindingFlags.NonPublic)?
                .GetValue(_sut);

            readDocumentsCountCache.Should().Be(batches.Sum(x => x.ReadDocumentsCount));
            failedReadDocumentsCountCache.Should().Be(batches.Sum(x => x.FailedReadDocumentsCount));
        }
    }
}
