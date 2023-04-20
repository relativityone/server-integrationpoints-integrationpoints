using System;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.InstanceSettings;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Kepler;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Services;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    public class JobProgressHandlerTests
    {
        private Mock<IImportJobController> _importJobController;
        private Mock<IKeplerServiceFactory> _serviceFactory;
        private Mock<IJobHistoryService> _jobHistoryService;
        private FakeTimer _timer;
        private Mock<ITimerFactory> _timerFactory;
        private Mock<IInstanceSettings> _instanceSettings;

        private JobProgressHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _importJobController = new Mock<IImportJobController>();

            _serviceFactory = new Mock<IKeplerServiceFactory>();
            _serviceFactory
                .Setup(x => x.CreateProxyAsync<IImportJobController>())
                .ReturnsAsync(_importJobController.Object);

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

            _sut = new JobProgressHandler(_serviceFactory.Object, _jobHistoryService.Object, _timerFactory.Object,
                _instanceSettings.Object, logger.Object);
        }

        [Test]
        public async Task BeginUpdateAsync_ShouldUpdateProgress()
        {
            // Arrange
            int workspaceId = 111;
            int jobHistoryId = 222;
            Guid importJobId = Guid.NewGuid();
            TimeSpan fakeUpdateInterval = TimeSpan.MaxValue;

            int totalRecords = 100;
            int firstImportedRecords = 5;
            int firstErroredRecords = 3;
            int secondImportedRecords = 15;
            int secondErroredRecords = 9;
            
            _instanceSettings
                .Setup(x => x.GetCustomProviderProgressUpdateIntervalAsync())
                .ReturnsAsync(fakeUpdateInterval);

            _importJobController
                .SetupSequence(x => x.GetProgressAsync(workspaceId, importJobId))
                .ReturnsAsync(new ValueResponse<ImportProgress>(importJobId, true, null, null, new ImportProgress(totalRecords, firstImportedRecords, firstErroredRecords)))
                .ReturnsAsync(new ValueResponse<ImportProgress>(importJobId, true, null, null, new ImportProgress(totalRecords, secondImportedRecords, secondErroredRecords)));
            
            // Act
            await _sut.BeginUpdateAsync(workspaceId, importJobId, jobHistoryId).ConfigureAwait(false);
            _timer.Callback(null);
            _timer.Callback(null);

            // Assert
            _jobHistoryService.Verify(x => x.UpdateProgressAsync(workspaceId, jobHistoryId, firstImportedRecords, firstErroredRecords));
            _jobHistoryService.Verify(x => x.UpdateProgressAsync(workspaceId, jobHistoryId, secondImportedRecords, secondErroredRecords));
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
