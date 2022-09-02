using System;
using System.Collections.Generic;
using Castle.Facilities.TypedFactory.Internal;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Monitoring.SystemReporter;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Environmental;
using Relativity.Services.ResourceServer;

namespace kCura.IntegrationPoints.Agent.Tests.Monitoring
{
    [TestFixture, Category("Unit")]
    public class SystemHealthReporterTests
    {
        private IDiskUsageReporter _diskUsageReporterFake;
        private Mock<IDiskUsageReporter> _diskUsageReporterMock;
        private Mock<IServicesMgr> _servicesMgrFake;
        private Mock<IFileShareServerManager> _fileShareServerManagerMock;
        private Mock<IPingService> _pingServiceMock;
        private Mock<IHelper> _helperMock;
        private Mock<IKeplerPingReporter> _keplerPingReporterMock;
        private IKeplerPingReporter _keplerPingReporterFake;
        private Mock<IDatabasePingReporter> _databasePingReporterMock;
        private Mock<IWorkspaceDBContext> _context;
        private IDatabasePingReporter _databasePingReporterFake;
        private Mock<IAPILog> _loggerMock;
        private SystemHealthReporter _sut;
        private Exception _exception = new Exception();

        [SetUp]
        public void SetUp()
        {
            _diskUsageReporterMock = new Mock<IDiskUsageReporter>();
            _helperMock = new Mock<IHelper>();
            _loggerMock = new Mock<IAPILog>();
            _diskUsageReporterFake = new DiskUsageReporter(_helperMock.Object, _loggerMock.Object);
            _fileShareServerManagerMock = new Mock<IFileShareServerManager>();
            _pingServiceMock = new Mock<IPingService>();
            _servicesMgrFake = new Mock<IServicesMgr>();
            _servicesMgrFake.Setup(s => s.CreateProxy<IFileShareServerManager>(ExecutionIdentity.System)).Returns(_fileShareServerManagerMock.Object);
            _servicesMgrFake.Setup(s => s.CreateProxy<IPingService>(ExecutionIdentity.System)).Returns(_pingServiceMock.Object);
            _helperMock.Setup(h => h.GetServicesManager()).Returns(_servicesMgrFake.Object);
            _keplerPingReporterMock = new Mock<IKeplerPingReporter>();
            _keplerPingReporterFake = new KeplerPingReporter(_helperMock.Object, _loggerMock.Object);
            _databasePingReporterMock = new Mock<IDatabasePingReporter>();
            _context = new Mock<IWorkspaceDBContext>();
            _databasePingReporterFake = new DatabasePingReporter(_context.Object, _loggerMock.Object);
            _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterMock.Object, _databasePingReporterMock.Object, _loggerMock.Object);
        }


        [Test]
        public void SystemHealthReporter_ShouldSendFileShareUsage_WhenDiskUsageReporterThrows()
        {
            // ARRANGE
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "FileServerListNotAvailableOrEmpty", 0 }
            };

            _helperMock.Setup(h => h.GetServicesManager()).Throws(_exception);
            _sut = new SystemHealthReporter(_diskUsageReporterFake, _keplerPingReporterMock.Object, _databasePingReporterMock.Object, _loggerMock.Object);


            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatistics();

            // ASSERT
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendFileShareUsage_WhenDiskUsageReporterReturnsEmptyList()
        {
            // ARRANGE
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "FileServerListNotAvailableOrEmpty", 0 }
            };
            var emptyResultSet = new FileShareQueryResultSet();

            _fileShareServerManagerMock.Setup(x => x.QueryAsync(It.IsAny<Query>())).ReturnsAsync(emptyResultSet);
            _sut = new SystemHealthReporter(_diskUsageReporterFake, _keplerPingReporterMock.Object, _databasePingReporterMock.Object, _loggerMock.Object);

            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatistics();

            // ASSERT
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendFileShareUsage_WhenDiskUsageReporterReturnsResults()
        {
            // ARRANGE
            _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterMock.Object, _databasePingReporterMock.Object, _loggerMock.Object);
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "files1.kcura.com", 50 },
                { "files2.kcura.com", 50 }
            };
            _diskUsageReporterMock.Setup(x => x.GetFileShareUsage()).Returns(expectedResult);

            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatistics();

            // ASSERT
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendSystemDiscUsage()
        {
            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatistics();

            // ASSERT
            result.Should().ContainKey(@"SystemDiscC:\Usage");
            result.Should().ContainKey(@"SystemDiscC:\FreeSpaceGB");
        }

        [Test]
        public void SystemHealthReporter_ShouldSendCPUUsage()
        {
            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatistics();

            // ASSERT
            result.Should().ContainKey("CpuUsageSystem");
        }

        [Test]
        public void SystemHealthReporter_ShouldSendSystemMemoryStatistics()
        {
            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatistics();

            // ASSERT
            result.Should().ContainKey("SystemFreeMemoryPercentage");
        }

        [Test]
        public void SystemHealthReporter_ShouldSendKeplerServiceStatusTrue_WhenStatusIsOk()
        {
            // ARRANGE
            _pingServiceMock.Setup(x => x.Ping()).ReturnsAsync("OK");
            _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterFake, _databasePingReporterMock.Object, _loggerMock.Object);

            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatistics();

            // ASSERT
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsKeplerServiceAccessible", true }
            };
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendKeplerServiceStatusFalse_WhenStatusIsDown()
        {
            // ARRANGE
            _pingServiceMock.Setup(x => x.Ping()).ReturnsAsync("Down");
            _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterFake, _databasePingReporterMock.Object, _loggerMock.Object);

            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatistics();

            // ASSERT
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsKeplerServiceAccessible", false }
            };
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendKeplerServiceStatusFalse_WhenServiceThrows()
        {
            // ARRANGE
            _helperMock.Setup(h => h.GetServicesManager()).Throws(_exception);
            _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterFake, _databasePingReporterMock.Object, _loggerMock.Object);

            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatistics();

            // ASSERT
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsKeplerServiceAccessible", false }
            };
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendDatabaseStatusTrue_WhenDatabaseIsOk()
        {
            // ARRANGE
            _context.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>())).Returns(1);
            _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterMock.Object, _databasePingReporterFake, _loggerMock.Object);

            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatistics();

            // ASSERT
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsDatabaseAccessible", true }
            };
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendDatabaseStatusFalse_WhenDatabaseResponseIsNotAsExpected()
        {
            // ARRANGE
            _context.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>())).Returns(69);
            _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterMock.Object, _databasePingReporterFake, _loggerMock.Object);

            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatistics();

            // ASSERT
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsDatabaseAccessible", false }
            };
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendDatabaseStatusFalse_WhenDatabaseThrows()
        {
            // ARRANGE
            _context.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>())).Throws(_exception);
            _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterMock.Object, _databasePingReporterFake, _loggerMock.Object);

            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatistics();

            // ASSERT
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsDatabaseAccessible", false }
            };
            result.Should().Contain(expectedResult);
        }

    }
}
