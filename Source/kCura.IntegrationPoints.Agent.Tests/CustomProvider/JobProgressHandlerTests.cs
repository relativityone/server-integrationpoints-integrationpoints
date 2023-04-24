using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.InstanceSettings;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1.Models;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    public class JobProgressHandlerTests
    {
        private Mock<IImportApiService> _importApiService;
        private Mock<IJobHistoryService> _jobHistoryService;
        private Mock<ITimerFactory> _timerFactory;
        private Mock<IInstanceSettings> _instanceSettings;

        private FakeTimer _timer;

        private JobProgressHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _importApiService = new Mock<IImportApiService>();
            _jobHistoryService = new Mock<IJobHistoryService>();
            _jobHistoryService = new Mock<IJobHistoryService>();

            _timer = new FakeTimer();

            _timerFactory = new Mock<ITimerFactory>();
            _timerFactory
                .Setup(x => x.Create(It.IsAny<TimerCallback>(), It.IsAny<object>(), It.IsAny<TimeSpan>(),
                    It.IsAny<TimeSpan>(), It.IsAny<string>()))
                .Returns((TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period, string name) =>
                {
                    _timer.Callback = callback;
                    return _timer;
                });

            _instanceSettings = new Mock<IInstanceSettings>();

            Mock<IAPILog> logger = new Mock<IAPILog>();
            logger.Setup(x => x.ForContext<JobProgressHandler>()).Returns(logger.Object);

            _sut = new JobProgressHandler(_importApiService.Object, _jobHistoryService.Object, _timerFactory.Object,
                _instanceSettings.Object, logger.Object);
        }

        [Test]
        public async Task BeginUpdateAsync_ShouldUpdateProgress()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            long ripJobId = 333;
            Guid importJobId = Guid.NewGuid();
            TimeSpan fakeUpdateInterval = TimeSpan.MaxValue;

            int totalRecords = 100;
            int firstImportedRecords = 5;
            int firstErroredRecords = 3;
            int secondImportedRecords = 15;
            int secondErroredRecords = 9;

            ImportJobContext importJobContext = new ImportJobContext(workspaceId, ripJobId, importJobId, jobHistoryId);
            
            _instanceSettings
                .Setup(x => x.GetCustomProviderProgressUpdateIntervalAsync())
                .ReturnsAsync(fakeUpdateInterval);

            _importApiService
                .SetupSequence(x => x.GetJobImportProgressValueAsync(It.Is<ImportJobContext>(context => context == importJobContext)))
                .ReturnsAsync(new ImportProgress(totalRecords, firstImportedRecords, firstErroredRecords))
                .ReturnsAsync(new ImportProgress(totalRecords, secondImportedRecords, secondErroredRecords));
            
            // Act
            await _sut.BeginUpdateAsync(importJobContext).ConfigureAwait(false);
            _timer.Callback(null);
            _timer.Callback(null);

            // Assert
            _jobHistoryService.Verify(x => x.UpdateProgressAsync(workspaceId, jobHistoryId, firstImportedRecords, firstErroredRecords));
            _jobHistoryService.Verify(x => x.UpdateProgressAsync(workspaceId, jobHistoryId, secondImportedRecords, secondErroredRecords));
        }

        [Test]
        public async Task SafeUpdateProgressAsync_ShouldSendUpdateProgressRequest()
        {
            // Arrange
            ImportProgress progress = new ImportProgress(30, 10, 20);
            ImportJobContext importJobContext = new ImportJobContext(1, 2, Guid.NewGuid(), 3);

            _importApiService
                .Setup(x => x.GetJobImportProgressValueAsync(importJobContext))
                .ReturnsAsync(progress);

            // Act
            await _sut.SafeUpdateProgressAsync(importJobContext).ConfigureAwait(false);

            // Assert
            _jobHistoryService.Verify(x => x.UpdateProgressAsync(importJobContext.WorkspaceId, importJobContext.JobHistoryId, progress.ImportedRecords, progress.ErroredRecords));
        }

        [Test]
        public async Task SafeUpdateProgressAsync_ShouldNotThrow()
        {
            // Arrange
            _importApiService
                .Setup(x => x.GetJobImportProgressValueAsync(It.IsAny<ImportJobContext>()))
                .Throws<InvalidOperationException>();

            // Act
            Func<Task> action = () => _sut.SafeUpdateProgressAsync(new ImportJobContext(1, 2, Guid.NewGuid(), 3));

            // Assert
            action.ShouldNotThrow();
        }

        [Test]
        public async Task UpdateReadItemsCountAsync_ShouldSendUpdateRequest()
        {
            // Arrange
            Job job = new Job()
            {
                WorkspaceID = 111
            };
            CustomProviderJobDetails jobDetails = new CustomProviderJobDetails()
            {
                JobHistoryID = 222,
                Batches = new List<CustomProviderBatch>()
                {
                    new CustomProviderBatch()
                    {
                        NumberOfRecords = 10,
                        IsAddedToImportQueue = true
                    },
                    new CustomProviderBatch()
                    {
                        NumberOfRecords = 20,
                        IsAddedToImportQueue = true
                    },
                    new CustomProviderBatch()
                    {
                        NumberOfRecords = 50,
                        IsAddedToImportQueue = false
                    }
                }
            };

            // Act
            await _sut.UpdateReadItemsCountAsync(job, jobDetails);

            // Assert
            _jobHistoryService
                .Verify(x => x.UpdateReadItemsCountAsync(job.WorkspaceID, jobDetails.JobHistoryID, 30));
        }

        [Test]
        public async Task SetTotalItemsAsync_ShouldPassthroughToJobHistoryService()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            int totalItemsCount = 333;

            // Act
            await _sut.SetTotalItemsAsync(workspaceId, jobHistoryId, totalItemsCount);

            // Assert
            _jobHistoryService.Verify(x => x.SetTotalItemsAsync(workspaceId, jobHistoryId, totalItemsCount));
        }

        private class FakeTimer : ITimer
        {
            public TimerCallback Callback { get; set; }
            
            public void Dispose()
            {
            }

            public bool Change(int dueTime, int period)
            {
                return true;
            }
        }
    }
}
