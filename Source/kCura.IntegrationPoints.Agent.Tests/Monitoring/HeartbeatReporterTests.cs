using kCura.IntegrationPoint.Tests.Core.Queries;
using kCura.IntegrationPoints.Agent.Monitoring;
using kCura.IntegrationPoints.Agent.Monitoring.HearbeatReporter;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Threading;

namespace kCura.IntegrationPoints.Agent.Tests.Monitoring
{
    public class HeartbeatReporterTests
    {
        private Mock<IQueueQueryManager> _queueManagerMock;
        private Mock<IMonitoringConfig> _configFake;
        private Mock<IDateTime> _dateTime;

        private HeartbeatReporter _sut;

        private readonly DateTime _EXPECTED_HEARTBEAT_TIME = DateTime.Today;
        private const long _JOB_ID = 10;

        [SetUp]
        public void SetUp()
        {
            Mock<IAPILog> log = new Mock<IAPILog>();

            _queueManagerMock = new Mock<IQueueQueryManager>();

            _configFake = new Mock<IMonitoringConfig>();

            _dateTime = new Mock<IDateTime>();
            _dateTime.Setup(x => x.UtcNow).Returns(_EXPECTED_HEARTBEAT_TIME);

            _sut = new HeartbeatReporter(_queueManagerMock.Object, _configFake.Object, _dateTime.Object, log.Object);
        }

        [Test]
        public void ActivateHeartbeat_ShouldUpdateHeartbeat_AfterActivation()
        {
            // Arrange
            _queueManagerMock.Setup(x => x.Heartbeat(It.IsAny<long>(), It.IsAny<DateTime>()))
                .Returns(new ValueReturnQuery<int>(1));

            _configFake.Setup(x => x.HeartbeatInterval).Returns(TimeSpan.FromDays(1));

            // Act
            using (_sut.ActivateHeartbeat(_JOB_ID)) { }

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

            _dateTime.Setup(x => x.UtcNow).Returns(newTime);
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
    }
}
