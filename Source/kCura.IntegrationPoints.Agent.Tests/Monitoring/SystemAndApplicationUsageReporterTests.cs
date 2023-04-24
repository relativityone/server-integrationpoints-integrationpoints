using System;
using System.Collections.Generic;
using System.Threading;
using kCura.IntegrationPoints.Agent.Monitoring;
using kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Metrics;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Agent.Tests.Monitoring
{
    [TestFixture, Category("Unit")]
    public class SystemAndApplicationUsageReporterTests
    {
        private const string _jobDetails = "jobDetails";
        private const string _jobType = "jobId";
        private const long _jobId = 123456789;
        private const int _dummyMemorySize = 12345;
        private Mock<IAPM> _apmMock;
        private Mock<ILogger<SystemAndApplicationUsageReporter>> _loggerMock;
        private Mock<IRipMetrics> _ripMetricMock;
        private Mock<ICounterMeasure> _counterMeasure;
        private Mock<IProcessMemoryHelper> _processMemoryHelper;
        private Mock<IMonitoringConfig> _configFake;
        private Mock<ISystemHealthReporter> _systemHealthReporterMock;
        private Mock<IAppDomainMonitoringEnabler> _appDomainMonitoringEnablerMock;
        private Mock<IRemovableAgent> _agentMock;
        private Mock<IRipToggleProvider> _toggleProviderFake;
        private Mock<ITimerFactory> _timerFactory;
        private TimerFake _timer;
        private SystemAndApplicationUsageReporter _sut;
        private readonly TimeSpan _memoryUsageInterval = TimeSpan.FromMilliseconds(10);
        private readonly Guid _agentInstanceGuid = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            _configFake = new Mock<IMonitoringConfig>();
            _configFake.Setup(x => x.MemoryUsageInterval).Returns(_memoryUsageInterval);

            _counterMeasure = new Mock<ICounterMeasure>();
            _loggerMock = new Mock<ILogger<SystemAndApplicationUsageReporter>>();
            _ripMetricMock = new Mock<IRipMetrics>();
            _apmMock = new Mock<IAPM>();
            _processMemoryHelper = new Mock<IProcessMemoryHelper>();
            _systemHealthReporterMock = new Mock<ISystemHealthReporter>();
            _appDomainMonitoringEnablerMock = new Mock<IAppDomainMonitoringEnabler>();
            _agentMock = new Mock<IRemovableAgent>();

            _apmMock.Setup(x => x.CountOperation(It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<int?>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<IEnumerable<ISink>>()))
                .Returns(_counterMeasure.Object);

            _ripMetricMock.Setup(x => x.GetWorkflowId()).Returns("workflowId");

            _processMemoryHelper.Setup(x => x.GetApplicationSystemStatistics()).Returns(
                new Dictionary<string, object>()
                {
                    { "SystemProcessMemoryInMB", _dummyMemorySize },
                    { "AppDomainMemoryInMB", _dummyMemorySize },
                    { "AppDomainLifetimeTotalAllocatedMemoryInMB", _dummyMemorySize },
                    { "PrivateMemoryInMB", _dummyMemorySize },
                    { "SystemFreeMemoryPercentage",  _dummyMemorySize },
                    { "CpuUsageSystem",  _dummyMemorySize },
                    { "CpuUsageProcess",  _dummyMemorySize}
                });

            _systemHealthReporterMock.Setup(x => x.GetSystemHealthStatisticsAsync()).ReturnsAsync(
                new Dictionary<string, object>()
                {
                    { "SystemHealth", _dummyMemorySize }
                });

            _appDomainMonitoringEnablerMock.Setup(x => x.EnableMonitoring()).Returns(true);

            _agentMock.Setup(x => x.ToBeRemoved).Returns(false);
            _agentMock.Setup(x => x.AgentInstanceGuid).Returns(_agentInstanceGuid);

            _toggleProviderFake = new Mock<IRipToggleProvider>();
            _toggleProviderFake.Setup(x => x.IsEnabled<EnableMemoryUsageReportingToggle>()).Returns(true);

            _timer = new TimerFake();

            _timerFactory = new Mock<ITimerFactory>();
            _timerFactory
                .Setup(x => x.Create(It.IsAny<TimerCallback>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<string>()))
                .Returns((TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period, string name) =>
                {
                    _timer.SetCallback(callback);
                    _timer.InvokeCallback();
                    return _timer;
                });

            _sut = new SystemAndApplicationUsageReporter(
                _apmMock.Object,
                _processMemoryHelper.Object,
                _appDomainMonitoringEnablerMock.Object,
                _configFake.Object,
                _agentMock.Object,
                _toggleProviderFake.Object,
                _timerFactory.Object,
                _systemHealthReporterMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Execute_ShouldSendMetrics_AfterTimerActivation()
        {
            // Arrange

            // Act
            using (_sut.ActivateTimer(_jobId, _jobDetails, _jobType))
            {
            }

            // Assert
            _apmMock.Verify(x => x.CountOperation(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<IEnumerable<ISink>>()));

            _loggerMock.Verify(x => x.LogInformation(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()));

            _counterMeasure.Verify(x => x.Write());
        }

        [Test]
        public void Execute_ShouldNotSendMetrics_AfterDisposingTimer()
        {
            // Act - activate timer
            IDisposable subscription = _sut.ActivateTimer(_jobId, _jobDetails, _jobType);

            // Assert that timer was activated
            _apmMock.Verify(
                x => x.CountOperation(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<IEnumerable<ISink>>()), Times.AtLeastOnce);

            // Act - dispose timer
            subscription.Dispose();

            _apmMock.ResetCalls();
            _counterMeasure.ResetCalls();
            _loggerMock.ResetCalls();

            // Simulate timer tick
            _timer.InvokeCallback();

            // Assert that no calls have been made after timer disposal
            _apmMock.Verify(
                x => x.CountOperation(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<IEnumerable<ISink>>()), Times.Never);

            _loggerMock.Verify(
                x => x.LogInformation(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()), Times.Never);

            _counterMeasure.Verify(x => x.Write(), Times.Never);
        }

        [Test]
        public void Execute_ShouldSendProperNumberOfMetrics_EvenIfErrorOcucuredSometimes()
        {
            // Arrange
            Mock<IAPM> apmMockWithErrors = new Mock<IAPM>();
            apmMockWithErrors.SetupSequence(x => x.CountOperation(
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<int?>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<IEnumerable<ISink>>()))
                .Throws<Exception>()
                .Returns(_counterMeasure.Object)
                .Throws<Exception>()
                .Returns(_counterMeasure.Object)
                .Returns(_counterMeasure.Object);

            SystemAndApplicationUsageReporter sutWithErrors = new SystemAndApplicationUsageReporter(apmMockWithErrors.Object,
                _processMemoryHelper.Object, _appDomainMonitoringEnablerMock.Object, _configFake.Object, _agentMock.Object,
                _toggleProviderFake.Object, _timerFactory.Object, _systemHealthReporterMock.Object, _loggerMock.Object);

            int metricsProperlySend = 3;
            int metricsWithError = 2;
            const string errorMessage = "An error occurred in Execute while sending APM metric";

            // Act
            sutWithErrors.ActivateTimer(_jobId, _jobDetails, _jobType);
            for (int i = 0; i < metricsProperlySend + metricsWithError; i++)
            {
                _timer.InvokeCallback();
            }

            // Assert
            apmMockWithErrors.Verify(x => x.CountOperation(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<IEnumerable<ISink>>()), Times.AtLeast(metricsProperlySend));

            _loggerMock.Verify(x => x.LogInformation(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()), Times.AtLeast(metricsProperlySend));

            _loggerMock.Verify(x => x.LogError(
                It.IsAny<Exception>(),
                It.Is<string>(mess => mess == errorMessage)), Times.AtLeast(metricsWithError));

            _counterMeasure.Verify(x => x.Write());
        }

        [Test]
        public void Execute_ShouldSendMetricsWithExpectedData_AfterTimerActivating()
        {
            // Arrange
            string metricName = "IntegrationPoints.Performance.System";
            string logMessage = "Sending metric {@metricName} with properties: {@MetricProperties} and correlationID: {correlationID}";

            // Act
            _sut.ActivateTimer(_jobId, _jobDetails, _jobType);

            // Assert
            _apmMock.Verify(x => x.CountOperation(
                It.Is<string>(name => name == metricName),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.Is<Dictionary<string, object>>(dictionary => CheckIfHasAllValues(dictionary)),
                It.IsAny<IEnumerable<ISink>>()));

            _loggerMock.Verify(x => x.LogInformation(
                It.Is<string>(message => message == logMessage),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()));

            _counterMeasure.Verify(x => x.Write());
        }

        [Test]
        public void Execute_ShouldNotStartTheTimer_WhenEnableMonitoringIsNotWorking()
        {
            // Arrange
            _appDomainMonitoringEnablerMock.Setup(x => x.EnableMonitoring()).Returns(false);

            // Act
            _sut.ActivateTimer(_jobId, _jobDetails, _jobType);

            // Assert
            _apmMock.Verify(x => x.CountOperation(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<IEnumerable<ISink>>()), Times.Never);
        }

        [Test]
        public void Execute_ShouldNotStartTheTimer_ToggleIsDisabled()
        {
            // Arrange
            _toggleProviderFake.Setup(x => x.IsEnabled<EnableMemoryUsageReportingToggle>()).Returns(false);

            // Act
            _sut.ActivateTimer(_jobId, _jobDetails, _jobType);

            // Assert
            _apmMock.Verify(x => x.CountOperation(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<IEnumerable<ISink>>()), Times.Never);
        }

        [Test]
        public void Execute_ShouldNotSendMetrics_WhenAgentToBeRemovedIsSetToTrue()
        {
            // Arrange
            _agentMock.Setup(x => x.ToBeRemoved).Returns(true);

            // Act
            _sut.ActivateTimer(_jobId, _jobDetails, _jobType);

            // Assert
            _apmMock.Verify(x => x.CountOperation(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<IEnumerable<ISink>>()), Times.Never);
        }

        private bool CheckIfHasAllValues(Dictionary<string, object> dict)
        {
            Dictionary<string, object> valuesToBeSend = new Dictionary<string, object>
            {
                { "r1.team.id", "PTCI-2456712" },
                { "service.name", "integrationpoints-repo" },
                { "r1.job.id", _jobId.ToString() },
                { "JobType", _jobType },
                { "WorkflowId", _jobDetails },
                { "SystemProcessMemoryInMB", _dummyMemorySize },
                { "AppDomainMemoryInMB", _dummyMemorySize },
                { "AppDomainLifetimeTotalAllocatedMemoryInMB", _dummyMemorySize },
                { "PrivateMemoryInMB", _dummyMemorySize },
                { "SystemFreeMemoryPercentage",  _dummyMemorySize },
                { "CpuUsageSystem",  _dummyMemorySize },
                { "CpuUsageProcess",  _dummyMemorySize}
            };

            foreach (var val in valuesToBeSend)
            {
                if (!dict.ContainsKey(val.Key) && (dict[val.Key] != val.Value))
                {
                    return false;
                }
            }
            return true;
        }

        private class TimerFake : ITimer
        {
            private TimerCallback _callback;
            private bool _disposed = false;

            public void Dispose()
            {
                _disposed = true;
            }

            public bool Change(int dueTime, int period)
            {
                return true;
            }

            public void SetCallback(TimerCallback callback)
            {
                _callback = callback;
            }

            public void InvokeCallback()
            {
                if (!_disposed && _callback != null)
                {
                    _callback(null);
                }
            }
        }
    }
}
