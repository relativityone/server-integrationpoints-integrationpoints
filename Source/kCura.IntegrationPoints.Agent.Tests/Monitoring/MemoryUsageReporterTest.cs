﻿using kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter;
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
        private Mock<IProcessMemoryHelper> _processMemoryHelper;
        private MemoryUsageReporter _sut;
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

            _sut = new MemoryUsageReporter(_apmMock.Object, _loggerMock.Object, _ripMetricMock.Object, _processMemoryHelper.Object);
        }

        [Test]
        public void Execute_ShouldSendMetrics_AfterTimerActivation()
        {
            // Arrange
            AppDomain.MonitoringIsEnabled = true;

            // Act
            IDisposable subscription = _sut.ActivateTimer(1, _jobId, _jobDetails, _jobType);
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
                It.IsAny<IEnumerable<ISink>>()));

            _loggerMock.Verify(x => x.LogInformation(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()));

            _counterMeasure.Verify(x => x.Write());

            subscription.Dispose();
        }

        [Test]
        public void Execute_ShouldNotSendMetrics_AfterDisposingTimer()
        {
            // Arrange
            AppDomain.MonitoringIsEnabled = true;

            // Act
            IDisposable subscription = _sut.ActivateTimer(1, _jobId, _jobDetails, _jobType);
            subscription.Dispose();
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

            MemoryUsageReporter sutWithErrors = new MemoryUsageReporter(apmMockWithErrors.Object, _loggerMock.Object, _ripMetricMock.Object, _processMemoryHelper.Object);

            AppDomain.MonitoringIsEnabled = true;
            int metricsProperlySend = 3;
            int metricsWithError = 2;
            const string errorMessage = "An error occured in Execute while sending APM metric";

            // Act
            sutWithErrors.ActivateTimer(1, _jobId, _jobDetails, _jobType);
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
            AppDomain.MonitoringIsEnabled = true;

            // Act
            IDisposable subscription = _sut.ActivateTimer(1, _jobId, _jobDetails, _jobType);
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