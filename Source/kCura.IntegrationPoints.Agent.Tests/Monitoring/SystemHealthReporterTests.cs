using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Monitoring.SystemReporter;
using kCura.IntegrationPoints.Data;
using Moq;
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
        private Mock<IServicesMgr> _servicesMgrFake;
        private Mock<IFileShareServerManager> _fileShareServerManagerMock;
        private FileShareDiskUsageReporter _fileShareDiskUsageReporterFake;
        private SystemStatisticsReporter _systemStatisticsReporterFake;
        private KeplerPingReporter _keplerPingReporterFake;
        private Mock<IPingService> _pingServiceMock;
        private Mock<IHelper> _helperMock;
        private Mock<IWorkspaceDBContext> _context;
        private Mock<IAPILog> _loggerMock;
        private SystemHealthReporter _sut;
        private Exception _exception = new Exception();

        [SetUp]
        public void SetUp()
        {
            _helperMock = new Mock<IHelper>();
            _loggerMock = new Mock<IAPILog>();
            _fileShareDiskUsageReporterFake = new FileShareDiskUsageReporter(_helperMock.Object, _loggerMock.Object);
            _fileShareServerManagerMock = new Mock<IFileShareServerManager>();
            _systemStatisticsReporterFake = new SystemStatisticsReporter(_loggerMock.Object);
            _keplerPingReporterFake = new KeplerPingReporter(_helperMock.Object, _loggerMock.Object);
            _pingServiceMock = new Mock<IPingService>();
            _servicesMgrFake = new Mock<IServicesMgr>();
            _servicesMgrFake.Setup(s => s.CreateProxy<IFileShareServerManager>(ExecutionIdentity.System)).Returns(_fileShareServerManagerMock.Object);
            _servicesMgrFake.Setup(s => s.CreateProxy<IPingService>(ExecutionIdentity.System)).Returns(_pingServiceMock.Object);
            _helperMock.Setup(h => h.GetServicesManager()).Returns(_servicesMgrFake.Object);
            new KeplerPingReporter(_helperMock.Object, _loggerMock.Object);
            _context = new Mock<IWorkspaceDBContext>();
            new DatabasePingReporter(_context.Object, _loggerMock.Object);
            _sut = new SystemHealthReporter(new List<IHealthStatisticReporter>());
        }


        [Test]
        public void SystemHealthReporter_ShouldNotSendFileShareUsage_WhenDiskUsageReporterThrows()
        {
            // ARRANGE
            _helperMock.Setup(h => h.GetServicesManager()).Throws(_exception);
            _sut = new SystemHealthReporter(new[] { _fileShareDiskUsageReporterFake });


            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // ASSERT
            result.Should().BeEmpty();
        }

        [Test]
        public void SystemHealthReporter_ShouldNotSendFileShareUsage_WhenDiskUsageReporterReturnsEmptyList()
        {
            // ARRANGE
            var emptyResultSet = new FileShareQueryResultSet();

            _fileShareServerManagerMock.Setup(x => x.QueryAsync(It.IsAny<Query>())).ReturnsAsync(emptyResultSet);
            _sut = new SystemHealthReporter(new[] { _fileShareDiskUsageReporterFake });

            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // ASSERT
            result.Should().BeEmpty();
        }

        [Test]
        public void SystemHealthReporter_ShouldSendSystemDiscUsage()
        {
            // ARRANGE
            _sut = new SystemHealthReporter(new[] { _systemStatisticsReporterFake });
            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // ASSERT
            result.Should().ContainKey(@"SystemDiscC:\Usage");
            result.Should().ContainKey(@"SystemDiscC:\FreeSpaceGB");
        }

        [Test]
        public void SystemHealthReporter_ShouldSendCPUUsage()
        {
            // ARRANGE
            _sut = new SystemHealthReporter(new[] { _systemStatisticsReporterFake });

            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // ASSERT
            result.Should().ContainKey("CpuUsageSystem");
        }

        [Test]
        public void SystemHealthReporter_ShouldSendSystemMemoryStatistics()
        {
            // ARRANGE
            _sut = new SystemHealthReporter(new[] { _systemStatisticsReporterFake });

            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // ASSERT
            result.Should().ContainKey("SystemFreeMemoryPercentage");
        }

        [Test]
        public void SystemHealthReporter_ShouldSendKeplerServiceStatusTrue_WhenStatusIsOk()
        {
            // ARRANGE
            _pingServiceMock.Setup(x => x.Ping()).ReturnsAsync("OK");
            _sut = new SystemHealthReporter(new[] { _keplerPingReporterFake });

            // ACT
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // ASSERT
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsKeplerServiceAccessible", true }
            };
            result.Should().Contain(expectedResult);
        }

        //
        // [Test]
        // public void SystemHealthReporter_ShouldSendKeplerServiceStatusFalse_WhenStatusIsDown()
        // {
        //     // ARRANGE
        //     _pingServiceMock.Setup(x => x.Ping()).ReturnsAsync("Down");
        //     _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterFake, _databasePingReporterMock.Object, _loggerMock.Object);
        //
        //     // ACT
        //     Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync();
        //
        //     // ASSERT
        //     Dictionary<string, object> expectedResult = new Dictionary<string, object>
        //     {
        //         { "IsKeplerServiceAccessible", false }
        //     };
        //     result.Should().Contain(expectedResult);
        // }
        //
        // [Test]
        // public void SystemHealthReporter_ShouldSendKeplerServiceStatusFalse_WhenServiceThrows()
        // {
        //     // ARRANGE
        //     _helperMock.Setup(h => h.GetServicesManager()).Throws(_exception);
        //     _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterFake, _databasePingReporterMock.Object, _loggerMock.Object);
        //
        //     // ACT
        //     Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync();
        //
        //     // ASSERT
        //     Dictionary<string, object> expectedResult = new Dictionary<string, object>
        //     {
        //         { "IsKeplerServiceAccessible", false }
        //     };
        //     result.Should().Contain(expectedResult);
        // }
        //
        // [Test]
        // public void SystemHealthReporter_ShouldSendDatabaseStatusTrue_WhenDatabaseIsOk()
        // {
        //     // ARRANGE
        //     _context.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>())).Returns(1);
        //     _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterMock.Object, _databasePingReporterFake, _loggerMock.Object);
        //
        //     // ACT
        //     Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync();
        //
        //     // ASSERT
        //     Dictionary<string, object> expectedResult = new Dictionary<string, object>
        //     {
        //         { "IsDatabaseAccessible", true }
        //     };
        //     result.Should().Contain(expectedResult);
        // }
        //
        // [Test]
        // public void SystemHealthReporter_ShouldSendDatabaseStatusFalse_WhenDatabaseResponseIsNotAsExpected()
        // {
        //     // ARRANGE
        //     _context.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>())).Returns(69);
        //     _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterMock.Object, _databasePingReporterFake, _loggerMock.Object);
        //
        //     // ACT
        //     Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync();
        //
        //     // ASSERT
        //     Dictionary<string, object> expectedResult = new Dictionary<string, object>
        //     {
        //         { "IsDatabaseAccessible", false }
        //     };
        //     result.Should().Contain(expectedResult);
        // }
        //
        // [Test]
        // public void SystemHealthReporter_ShouldSendDatabaseStatusFalse_WhenDatabaseThrows()
        // {
        //     // ARRANGE
        //     _context.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>())).Throws(_exception);
        //     _sut = new SystemHealthReporter(_diskUsageReporterMock.Object, _keplerPingReporterMock.Object, _databasePingReporterFake, _loggerMock.Object);
        //
        //     // ACT
        //     Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync();
        //
        //     // ASSERT
        //     Dictionary<string, object> expectedResult = new Dictionary<string, object>
        //     {
        //         { "IsDatabaseAccessible", false }
        //     };
        //     result.Should().Contain(expectedResult);
        // }

    }
}
