using kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter;
using kCura.IntegrationPoints.Common.Metrics;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Telemetry.APM;
using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Agent.Tests.Monitoring
{
    [TestFixture, Category("Unit")]
    public class MemoryUsageReporterTest
    {
        private Mock<IAPM> _apmMock;
        private Mock<IAPILog> _loggerMock;
        private Mock<IRipMetrics> _ripMetricMock;
        private Mock<ICounterMeasure> _counterMeasure;
        private Mock<IProcessMemoryHelper> _processMemoryHelper;
        private MemoryUsageReporter _sut;
        private TestScheduler _testScheduler;
        private const string _jobDetails = "jobDetails";
        private const string _jobType = "jobId";
        private const long _jobId = 123456789;
        private const int _dummyMemorySize = 12345;

        [SetUp]
        public void SetUp()
        {
            _counterMeasure = new Mock<ICounterMeasure>();
            _loggerMock = new Mock<IAPILog>();
            _ripMetricMock = new Mock<IRipMetrics>();
            _apmMock = new Mock<IAPM>();
            _processMemoryHelper = new Mock<IProcessMemoryHelper>();
            _testScheduler = new TestScheduler();

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

            _processMemoryHelper.Setup(x => x.GetCurrentProcessMemoryUsage()).Returns(_dummyMemorySize);
            _processMemoryHelper.Setup(x => x.GetApplicationSystemStats()).Returns(
                new Dictionary<string, object>()
                {
                    { "SystemProcessMemoryInMB", _dummyMemorySize },
                    { "AppDomainMemoryInMB", _dummyMemorySize },
                    { "AppDomainLifetimeTotalAllocatedMemoryInMB", _dummyMemorySize },
                    { "PrivateMemoryInMB", _dummyMemorySize },
                    { "SystemFreeMemoryPercent",  _dummyMemorySize}
                });

            _sut = new MemoryUsageReporter(_apmMock.Object, _loggerMock.Object, _ripMetricMock.Object, _processMemoryHelper.Object, _testScheduler);
        }

        [Test]
        public void Execute_ShouldSendMetricsExpectedNumberOfTimes_AfterTimerActivation()
        {
            // Arrange
            AppDomain.MonitoringIsEnabled = true;
            const int timesToBeInvoked = 5;

            // Act
            using(_sut.ActivateTimer(1, _jobId, _jobDetails, _jobType))
            {
                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(timesToBeInvoked).Ticks);
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
                It.IsAny<IEnumerable<ISink>>()), Times.Exactly(timesToBeInvoked));

            _loggerMock.Verify(x => x.LogInformation(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()), Times.Exactly(timesToBeInvoked));

            _counterMeasure.Verify(x => x.Write(), Times.Exactly(timesToBeInvoked));
        }

        [Test]
        public void Execute_ShouldStopSendingMetrics_AfterDisposingSubscription()
        {
            // Arrange
            AppDomain.MonitoringIsEnabled = true;
            const int timesToBeInvoked = 5;

            // Act
            IDisposable subscription = _sut.ActivateTimer(1, _jobId, _jobDetails, _jobType);
            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(timesToBeInvoked).Ticks);
            subscription.Dispose();
            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(timesToBeInvoked).Ticks);

            // Assert
            _apmMock.Verify(x => x.CountOperation(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<IEnumerable<ISink>>()), Times.Exactly(timesToBeInvoked));

            _loggerMock.Verify(x => x.LogInformation(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()), Times.Exactly(timesToBeInvoked));

            _counterMeasure.Verify(x => x.Write(), Times.Exactly(timesToBeInvoked));

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
                .Returns(_counterMeasure.Object)
                .Throws<Exception>()
                .Returns(_counterMeasure.Object)
                .Throws<Exception>()
                .Returns(_counterMeasure.Object);

            MemoryUsageReporter sutWithErrors = new MemoryUsageReporter(apmMockWithErrors.Object, _loggerMock.Object, _ripMetricMock.Object, _processMemoryHelper.Object, _testScheduler);

            AppDomain.MonitoringIsEnabled = true;
            const int timesToBeInvoked = 5;
            const int numberOfErrors = 2;
            const string errorMessage = "An error occured in Execute while sending APM metric";

            // Act
            using (sutWithErrors.ActivateTimer(1, _jobId, _jobDetails, _jobType))
            {
                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(timesToBeInvoked).Ticks);
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
                It.IsAny<IEnumerable<ISink>>()), Times.Exactly(timesToBeInvoked));

            _loggerMock.Verify(x => x.LogInformation(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()), Times.Exactly(timesToBeInvoked - numberOfErrors));

            _loggerMock.Verify(x => x.LogError(
                It.IsAny<Exception>(),
                It.Is<string>(mess => mess == errorMessage)), Times.Exactly(numberOfErrors));

            _counterMeasure.Verify(x => x.Write(), Times.Exactly(timesToBeInvoked- numberOfErrors));
        }

        [Test]
        public void Execute_ShouldSendMetricsWithExpectedData_AfterTimerActivating()
        {
            // Arrange
            string metricName = "IntegrationPoints.Performance.System";
            string logMessage = "Sending metric {@metricName} with properties: {@MetricProperties} and correlationID: {@CorrelationId}";
            AppDomain.MonitoringIsEnabled = true;

            // Act
            using (_sut.ActivateTimer(1, _jobId, _jobDetails, _jobType))
            {
                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
            }

            // Assert
            _apmMock.Verify(x => x.CountOperation(
                It.Is<string>(name => name == metricName),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.Is<Dictionary<string, object>>(dictionary => CheckIfHasAllValues(dictionary)),
                It.IsAny<IEnumerable<ISink>>()), Times.Once);

            _loggerMock.Verify(x => x.LogInformation(
                It.Is<string>(message => message == logMessage),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()), Times.Once);

            _counterMeasure.Verify(x => x.Write(), Times.Once);
        }

        private bool CheckIfHasAllValues(Dictionary<string, object> dict) 
        {
            Dictionary<string, object> valuesToBeSend = new Dictionary<string, object>
            {
                { "MemoryUsage", _dummyMemorySize},
                { "JobId", _jobId},
                { "JobType", _jobType},
                { "WorkflowId", _jobDetails},
                { "SystemProcessMemoryInMB", _dummyMemorySize },
                { "AppDomainMemoryInMB", _dummyMemorySize },
                { "AppDomainLifetimeTotalAllocatedMemoryInMB", _dummyMemorySize },
                { "PrivateMemoryInMB", _dummyMemorySize },
                { "SystemFreeMemoryPercent",  _dummyMemorySize}
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
