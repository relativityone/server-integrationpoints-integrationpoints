﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.RelativitySync.Metrics;
using kCura.ScheduleQueue.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync;
using Relativity.Sync.Executors.Validation;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.RelativitySync.Tests
{
    [TestFixture, Category("Unit")]
    public class RelativitySyncAdapterTests
    {
        private RelativitySyncAdapter _sut;

        private Mock<IJobHistorySyncService> _jobHistorySyncServiceMock;
        private Mock<ISyncOperationsWrapper> _syncOperationsMock;

        private Mock<ICancellationAdapter> _cancellationAdapterFake;
        private Mock<ISyncJob> _syncJobFake;
        private Mock<IAPILog> _logFake;

        [SetUp]
        public void SetUp()
        {
            _logFake = new Mock<IAPILog>();

            _jobHistorySyncServiceMock = new Mock<IJobHistorySyncService>();

            _cancellationAdapterFake = new Mock<ICancellationAdapter>();
            _cancellationAdapterFake.Setup(x => x.GetCancellationToken(It.IsAny<Action>()))
                .Returns(new CompositeCancellationToken(It.IsAny<CancellationToken>(), It.IsAny<CancellationToken>(), _logFake.Object));

            _syncJobFake = new Mock<ISyncJob>();

            _syncOperationsMock = PrepareSyncOperations();

            Mock<IExtendedJob> job = new Mock<IExtendedJob>();
            Mock<IAPM> apmMetrics = new Mock<IAPM>();
            Mock<ISyncJobMetric> metrics = new Mock<ISyncJobMetric>();
            Mock<IIntegrationPointToSyncConverter> integrationPointsToSyncConverter = new Mock<IIntegrationPointToSyncConverter>();

            _sut = new RelativitySyncAdapter(
                job.Object,
                _logFake.Object,
                apmMetrics.Object,
                metrics.Object,
                _jobHistorySyncServiceMock.Object,
                integrationPointsToSyncConverter.Object,
                _syncOperationsMock.Object,
                _cancellationAdapterFake.Object);
        }

        [Test]
        public async Task RunAsync_ShouldRunSyncAndMarkAsCompleted()
        {
            // Act
            TaskResult result = await _sut.RunAsync().ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(TaskStatusEnum.Success);

            _jobHistorySyncServiceMock.Verify(x => x.MarkJobAsCompletedAsync(It.IsAny<IExtendedJob>()));
        }

        [Test]
        public async Task RunAsync_ShouldRunSyncAndMarkJobAsCancelled_WhenCancellationWasRequested()
        {
            // Arrange
            SetupCancellation(isCanceled: true);

            // Act
            TaskResult result = await _sut.RunAsync().ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(TaskStatusEnum.Success);

            _jobHistorySyncServiceMock.Verify(x => x.MarkJobAsStoppedAsync(It.IsAny<IExtendedJob>()));
        }

        [Test]
        public async Task RunAsync_ShouldRunSyncAndMarkJobAsDrainStopped_WhenDrainStopWasRequested()
        {
            // Arrange
            SetupCancellation(isDrainStopped: true);

            // Act
            TaskResult result = await _sut.RunAsync().ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(TaskStatusEnum.DrainStopped);

            _jobHistorySyncServiceMock.Verify(x => x.MarkJobAsSuspendedAsync(It.IsAny<IExtendedJob>()));
        }

        [Test]
        public async Task RunAsync_ShouldRunSyncAndMarkJobAsValidationFailed_WhenValidationExceptionWasThrown()
        {
            // Arrange
            ValidationException expectedException = new ValidationException();

            _syncJobFake.Setup(x => x.ExecuteAsync(It.IsAny<IProgress<SyncJobState>>(), It.IsAny<CompositeCancellationToken>()))
                .Throws(expectedException);

            // Act
            TaskResult result = await _sut.RunAsync().ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(TaskStatusEnum.Fail);

            _jobHistorySyncServiceMock.Verify(x => x.MarkJobAsValidationFailedAsync(It.IsAny<IExtendedJob>(), expectedException));
        }

        [Test]
        public async Task RunAsync_ShouldMarkSyncConfigurationAsResuming_WhenJobWasResumed()
        {
            // Arrange
            const int expectedSyncConfigurationId = 10;

            _syncOperationsMock.Setup(x => x.TryGetResumedSyncConfigurationIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(expectedSyncConfigurationId);

            // Act
            await _sut.RunAsync().ConfigureAwait(false);

            // Assert
            _syncOperationsMock.Verify(x => x.PrepareSyncConfigurationForResumeAsync(It.IsAny<int>(), expectedSyncConfigurationId));
        }

        private Mock<ISyncOperationsWrapper> PrepareSyncOperations()
        {
            Mock<ISyncJobFactory> syncJobFactory = new Mock<ISyncJobFactory>();
            syncJobFactory
                .Setup(x => x.Create(It.IsAny<SyncJobParameters>(), It.IsAny<IRelativityServices>(), It.IsAny<IAPILog>()))
                .Returns(_syncJobFake.Object);

            Mock<ISyncOperationsWrapper> syncOperationsWrapper = new Mock<ISyncOperationsWrapper>();
            syncOperationsWrapper.Setup(x => x.CreateSyncJobFactory())
                .Returns(syncJobFactory.Object);

            return syncOperationsWrapper;
        }

        private void SetupCancellation(bool isCanceled = false, bool isDrainStopped = false)
        {
            CancellationToken stopToken = new CancellationToken(isCanceled);
            CancellationToken drainStopToken = new CancellationToken(isDrainStopped);

            _cancellationAdapterFake.Setup(x => x.GetCancellationToken(It.IsAny<Action>()))
                .Returns(new CompositeCancellationToken(stopToken, drainStopToken, _logFake.Object));
        }
    }
}
