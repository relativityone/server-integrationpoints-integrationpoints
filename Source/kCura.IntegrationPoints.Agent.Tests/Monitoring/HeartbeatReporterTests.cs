using System;
using System.Collections.Generic;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Queries;
using kCura.IntegrationPoints.Agent.Monitoring;
using kCura.IntegrationPoints.Agent.Monitoring.HearbeatReporter;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Agent.Tests.Monitoring
{
    public class HeartbeatReporterTests
    {
        private Mock<IQueueQueryManager> _queueManagerMock;
        private Mock<IMonitoringConfig> _configFake;
        private Mock<IDateTime> _dateTimeFake;
        private Mock<IAPM> _apmMock;
        private Mock<ICounterMeasure> _counterMeasure;
        private ITimerFactory _timerFactory;
        private HeartbeatReporter _sut;
        private readonly DateTime _EXPECTED_HEARTBEAT_TIME = DateTime.Today;
        private readonly string _METRIC_RUNNING_JOB_TIME_EXCEEDED_NAME = "Relativity.IntegrationPoints.Performance.RunningJobTimeExceeded";
        private const long _JOB_ID = 10;

        [SetUp]
        public void SetUp()
        {
            Mock<IAPILog> log = new Mock<IAPILog>();

            Mock<IQuery<int>> query = new Mock<IQuery<int>>();
            query.Setup(x => x.Execute()).Returns(0);

            _queueManagerMock = new Mock<IQueueQueryManager>();
            _queueManagerMock.Setup(x => x.Heartbeat(_JOB_ID, _EXPECTED_HEARTBEAT_TIME)).Returns(query.Object);

            _configFake = new Mock<IMonitoringConfig>();

            _dateTimeFake = new Mock<IDateTime>();
            _dateTimeFake.Setup(x => x.UtcNow).Returns(_EXPECTED_HEARTBEAT_TIME);

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

            _timerFactory = new TimerFactory(log.Object);

            _sut = new HeartbeatReporter(_queueManagerMock.Object, _configFake.Object, _dateTimeFake.Object, _apmMock.Object, _timerFactory, log.Object);
        }

        [Test]
        public void ActivateHeartbeat_ShouldUpdateHeartbeat_AfterActivation()
        {
            // Arrange
            _queueManagerMock.Setup(x => x.Heartbeat(It.IsAny<long>(), It.IsAny<DateTime>()))
                .Returns(new ValueReturnQuery<int>(1));

            _configFake.Setup(x => x.HeartbeatInterval).Returns(TimeSpan.FromDays(1));

            // Act
            using (_sut.ActivateHeartbeat(_JOB_ID))
            {
                Thread.Sleep(100);
            }

            // Assert
            _queueManagerMock.Verify(x => x.Heartbeat(_JOB_ID, _EXPECTED_HEARTBEAT_TIME), Times.Once);
        }

        [Test]
        public void ActivateHeartBeat_ShouldNotUpdateHeartbeat_AfterDisposing()
        {
            // Arrange
            _queueManagerMock.Setup(x => x.Heartbeat(It.IsAny<long>(), It.IsAny<DateTime>()))
                .Returns(new ValueReturnQuery<int>(1));

            _configFake.Setup(x => x.HeartbeatInterval).Returns(TimeSpan.FromMilliseconds(10));

            // Act
            using (_sut.ActivateHeartbeat(_JOB_ID)) { }

            // Assert
            DateTime newTime = new DateTime(1900, 10, 10);

            _dateTimeFake.Setup(x => x.UtcNow).Returns(newTime);
            Thread.Sleep(100);

            _queueManagerMock.Verify(x => x.Heartbeat(_JOB_ID, newTime), Times.Never);
        }

        [Test]
        public void ActivateHeartbeat_ShouldContinouslyUpdateHeartbeat_WhenExceptionWasThrown()
        {
            // Arrange
            _queueManagerMock.Setup(x => x.Heartbeat(It.IsAny<long>(), It.IsAny<DateTime>()))
                .Throws<Exception>();

            _configFake.Setup(x => x.HeartbeatInterval).Returns(TimeSpan.FromMilliseconds(10));

            // Act
            using (_sut.ActivateHeartbeat(_JOB_ID))
            {
                Thread.Sleep(100);
            }

            // Assert
            _queueManagerMock.Verify(x => x.Heartbeat(_JOB_ID, _EXPECTED_HEARTBEAT_TIME), Times.AtLeast(2));
        }

        [Test]
        public void Execute_ShouldSendMetric_WhenRunningJobTimeThresholdTimeIsExceeded()
        {
            // Arrange
            TimeSpan timeToExceedThreshold = TimeSpan.FromHours(10);
            _configFake.Setup(x => x.HeartbeatInterval).Returns(TimeSpan.FromMilliseconds(10));
            _configFake.Setup(x => x.LongRunningJobsTimeThreshold).Returns(TimeSpan.FromHours(8));

            // Act
            using (_sut.ActivateHeartbeat(_JOB_ID))
            {
                Thread.Sleep(100);
                _dateTimeFake.Setup(x => x.UtcNow).Returns(_EXPECTED_HEARTBEAT_TIME + timeToExceedThreshold);
                Thread.Sleep(100);
            }

            // Assert
            _apmMock.Verify(x => x.CountOperation(
                _METRIC_RUNNING_JOB_TIME_EXCEEDED_NAME,
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<IEnumerable<ISink>>()), Times.Once);
        }
    }
}
