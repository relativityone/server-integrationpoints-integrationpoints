using kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter;
using kCura.IntegrationPoints.Common.Metrics;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Telemetry.APM;
using System;
using System.Collections.Generic;
using System.Threading;

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

        [SetUp]
        public void SetUp()
        {
            _apmMock = new Mock<IAPM>();
            _apmMock.Setup(x => x.CountOperation(It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<int?>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<IEnumerable<ISink>>()))
                .Returns(Mock.Of<ICounterMeasure>());

            _counterMeasure = new Mock<ICounterMeasure>();

            _loggerMock = new Mock<IAPILog>();

            _ripMetricMock = new Mock<IRipMetrics>();
            _ripMetricMock.Setup(x => x.GetWorkflowId()).Returns("workflowId");

            _sut = new MemoryUsageReporter(_apmMock.Object, _loggerMock.Object, _ripMetricMock.Object);
        }

        [Test]
        public void Execute_ShouldSendMetrics_AfterTimerActivating()
        {
            // Arrange
            List<string> valuesToBeSend = new List<string>() 
            {
                "MemoryUsage",
                "JobId",
                "JobType",
                "WorkflowId",
                "SystemProcessMemory",
                "AppDomainMemory",
                "AppDomainLifetimeTotalAllocatedMemory",
                "PrivateMemoryBytes",
                "SystemFreeMemoryPercent"
            };
            string metricName = "Relativity.IntegrationPoints.Performance.System";
            string logMessage = "Sending metric {@metricName} with properties: {@MetricProperties} and correlationID: {@CorrelationId}";
            AppDomain.MonitoringIsEnabled = true;

            // Act
            IDisposable temp = _sut.ActivateTimer(1000, 1111111111111, "jobDetails", "jobType");
            Thread.Sleep(500);
            
            // Assert
            _apmMock.Verify(x => x.CountOperation(
                It.Is<string>(name => name == metricName),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.Is<Dictionary<string, object>>(dictionary => CheckIfHasAllValues(dictionary, valuesToBeSend)),
                It.IsAny<IEnumerable<ISink>>()));

            _loggerMock.Verify(x => x.LogInformation(
                It.Is<string>(message => message == logMessage),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()));

            _counterMeasure.Verify(x => x.Write());
        }

        private bool CheckIfHasAllValues(Dictionary<string, object> dict, List<string> values) 
        {
            foreach (string val in values)
            {
                if (!dict.ContainsKey(val))
                    return false;
            }
            return true;
        }
    }
}
