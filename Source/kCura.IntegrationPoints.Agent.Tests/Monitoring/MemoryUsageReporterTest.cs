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
        private MemoryUsageReporter _sut;
        private TestScheduler _testScheduler;

        [SetUp]
        public void SetUp()
        {
            _counterMeasure = new Mock<ICounterMeasure>();
            _apmMock = new Mock<IAPM>();
            _apmMock.Setup(x => x.CountOperation(It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<int?>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<IEnumerable<ISink>>()))
                .Returns(_counterMeasure.Object);

            _loggerMock = new Mock<IAPILog>();

            _ripMetricMock = new Mock<IRipMetrics>();
            _ripMetricMock.Setup(x => x.GetWorkflowId()).Returns("workflowId");

            _testScheduler = new TestScheduler();
            _sut = new MemoryUsageReporter(_apmMock.Object, _loggerMock.Object, _ripMetricMock.Object, _testScheduler);
        }

        [Test]
        public void Execute_ShouldSendMetricsExpectedNumberOfTimes_AfterTimerActivation()
        {
            // Arrange
            AppDomain.MonitoringIsEnabled = true;
            const int timesToBeInvoked = 5;
            const long jobId = 1111111111111;

            // Act
            using(_sut.ActivateTimer(1, jobId, "jobDetails", "jobType"))
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

            MemoryUsageReporter sutWithErrors = new MemoryUsageReporter(apmMockWithErrors.Object, _loggerMock.Object, _ripMetricMock.Object, _testScheduler);

            AppDomain.MonitoringIsEnabled = true;
            const int timesToBeInvoked = 5;
            const int numberOfErrors = 2;
            const long jobId = 1111111111111;
            const string errorMessage = "An error occured in Execute while sending APM metric";

            // Act
            using (sutWithErrors.ActivateTimer(1, jobId, "jobDetails", "jobType"))
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
            List<string> valuesToBeSend = new List<string>() 
            {
                "MemoryUsage",
                "JobId",
                "JobType",
                "WorkflowId",
                "SystemProcessMemoryInMB",
                "AppDomainMemoryInMB",
                "AppDomainLifetimeTotalAllocatedMemoryInMB",
                "PrivateMemoryInMB",
                "SystemFreeMemoryPercent"
            };
            string metricName = "IntegrationPoints.Performance.System";
            string logMessage = "Sending metric {@metricName} with properties: {@MetricProperties} and correlationID: {@CorrelationId}";
            AppDomain.MonitoringIsEnabled = true;

            // Act
            using (_sut.ActivateTimer(1, 1111111111111, "jobDetails", "jobType"))
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
                It.Is<Dictionary<string, object>>(dictionary => CheckIfHasAllValues(dictionary, valuesToBeSend)),
                It.IsAny<IEnumerable<ISink>>()), Times.Once);

            _loggerMock.Verify(x => x.LogInformation(
                It.Is<string>(message => message == logMessage),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()), Times.Once);

            _counterMeasure.Verify(x => x.Write(), Times.Once);
        }

        private bool CheckIfHasAllValues(Dictionary<string, object> dict, List<string> values) 
        {
            foreach (string val in values)
            {
                if (!dict.ContainsKey(val))
                {
                    return false;
                }   
            }
            return true;
        }
    }
}
