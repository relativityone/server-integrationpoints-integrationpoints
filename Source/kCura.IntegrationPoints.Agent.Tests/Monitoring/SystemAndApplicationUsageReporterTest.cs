﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.Monitoring;
using kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Common.Metrics;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Telemetry.APM;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.Tests.Monitoring
{
    [TestFixture, Category("Unit")]
    public class SystemAndApplicationUsageReporterTest
    {
        private const string _jobDetails = "jobDetails";
        private const string _jobType = "jobId";
        private const long _jobId = 123456789;
        private const int _dummyMemorySize = 12345;

        private Mock<IAPM> _apmMock;
        private Mock<IAPILog> _loggerMock;
        private Mock<IRipMetrics> _ripMetricMock;
        private Mock<ICounterMeasure> _counterMeasure;
        private Mock<IProcessMemoryHelper> _processMemoryHelper;
        private Mock<IMonitoringConfig> _configFake;
        private Mock<ISystemHealthReporter> _systemHealthReporterMock;
        private SystemAndApplicationUsageReporter _sut;
        private Mock<IAppDomainMonitoringEnabler> _appDomainMonitoringEnablerMock;
        private Mock<IRemovableAgent> _agentMock;
        private Mock<IToggleProvider> _toggleProviderFake;
        private ITimerFactory _timerFactory;

        private readonly TimeSpan _MEMORY_USAGE_INTERVAL = TimeSpan.FromMilliseconds(10);
        private readonly Guid _agentInstanceGuid = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            _configFake = new Mock<IMonitoringConfig>();
            _configFake.Setup(x => x.MemoryUsageInterval).Returns(_MEMORY_USAGE_INTERVAL);

            _counterMeasure = new Mock<ICounterMeasure>();
            _loggerMock = new Mock<IAPILog>();
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
                    { "SystemFreeMemoryPercentage",  _dummyMemorySize},
                    { "CpuUsageSystem",  _dummyMemorySize},
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

            _toggleProviderFake = new Mock<IToggleProvider>();
            _toggleProviderFake.Setup(x => x.IsEnabled<EnableMemoryUsageReportingToggle>()).Returns(true);

            _timerFactory = new TimerFactory(_loggerMock.Object);

            _sut = new SystemAndApplicationUsageReporter(_apmMock.Object, _ripMetricMock.Object,
                _processMemoryHelper.Object, _appDomainMonitoringEnablerMock.Object, _configFake.Object, _agentMock.Object,
                _toggleProviderFake.Object, _timerFactory, _systemHealthReporterMock.Object, _loggerMock.Object);
        }

        [Test]
        public void Execute_ShouldSendMetrics_AfterTimerActivation()
        {
            // Arrange

            // Act
            using (_sut.ActivateTimer(_jobId, _jobDetails, _jobType))
            {
                Thread.Sleep(100);
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
        public async Task Execute_ShouldNotSendMetrics_AfterDisposingTimer()
        {
            // Arrange
            _configFake.SetupGet(x => x.TimerStartDelay).Returns(TimeSpan.FromMilliseconds(10));

            // Act
            IDisposable subscription = _sut.ActivateTimer(_jobId, _jobDetails, _jobType);
            subscription.Dispose();
            await Task.Delay(20);

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

            _loggerMock.Verify(x => x.LogInformation(
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

            SystemAndApplicationUsageReporter sutWithErrors = new SystemAndApplicationUsageReporter(apmMockWithErrors.Object, _ripMetricMock.Object,
                _processMemoryHelper.Object, _appDomainMonitoringEnablerMock.Object, _configFake.Object, _agentMock.Object,
                _toggleProviderFake.Object, _timerFactory, _systemHealthReporterMock.Object, _loggerMock.Object);

            int metricsProperlySend = 3;
            int metricsWithError = 2;
            const string errorMessage = "An error occurred in Execute while sending APM metric";

            // Act
            sutWithErrors.ActivateTimer(_jobId, _jobDetails, _jobType);
            Thread.Sleep(1000);

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
            string logMessage = "Sending metric {@metricName} with properties: {@MetricProperties} and correlationID: {@CorrelationId}";

            // Act
            IDisposable subscription = _sut.ActivateTimer(_jobId, _jobDetails, _jobType);
            Thread.Sleep(10);

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
            //Arrange
            _appDomainMonitoringEnablerMock.Setup(x => x.EnableMonitoring()).Returns(false);

            //Act
            IDisposable subscription = _sut.ActivateTimer(_jobId, _jobDetails, _jobType);
            Thread.Sleep(10);

            //Assert
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
            //Arrange
            _toggleProviderFake.Setup(x => x.IsEnabled<EnableMemoryUsageReportingToggle>()).Returns(false);

            //Act
            IDisposable subscription = _sut.ActivateTimer(_jobId, _jobDetails, _jobType);
            Thread.Sleep(10);

            //Assert
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
            IDisposable subscription = _sut.ActivateTimer(_jobId, _jobDetails, _jobType);
            Thread.Sleep(100);

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

            subscription.Dispose();
        }

        private bool CheckIfHasAllValues(Dictionary<string, object> dict)
        {
            Dictionary<string, object> valuesToBeSend = new Dictionary<string, object>
            {
                { "r1.team.id", "PTCI-2456712" },
                { "r1.job.id", _jobId.ToString()},
                { "JobType", _jobType},
                { "WorkflowId", _jobDetails},
                { "SystemProcessMemoryInMB", _dummyMemorySize },
                { "AppDomainMemoryInMB", _dummyMemorySize },
                { "AppDomainLifetimeTotalAllocatedMemoryInMB", _dummyMemorySize },
                { "PrivateMemoryInMB", _dummyMemorySize },
                { "SystemFreeMemoryPercentage",  _dummyMemorySize},
                { "CpuUsageSystem",  _dummyMemorySize},
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
    }
}