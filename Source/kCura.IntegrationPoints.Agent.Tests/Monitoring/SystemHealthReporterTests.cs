using System;
using System.Collections.Generic;
using System.Data;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter;
using kCura.IntegrationPoints.Data.DbContext;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Environmental;
using Relativity.Services.ResourceServer;

namespace kCura.IntegrationPoints.Agent.Tests.Monitoring
{
    [TestFixture]
    [Category("Unit")]
    public class SystemHealthReporterTests
    {
        private Mock<IServicesMgr> _servicesMgrFake;
        private Mock<IFileShareServerManager> _fileShareServerManagerMock;
        private SystemStatisticsReporter _systemStatisticsReporterFake;
        private KeplerPingReporter _keplerPingReporterFake;
        private Mock<IPingService> _pingServiceMock;
        private Mock<IHelper> _helperMock;
        private DatabasePingReporter _databasePingReporterFake;
        private Mock<IEddsDBContext> _context;
        private Mock<IAPILog> _loggerMock;
        private SystemHealthReporter _sut;
        private Exception _exception = new Exception();

        [SetUp]
        public void SetUp()
        {
            _helperMock = new Mock<IHelper>();
            _loggerMock = new Mock<IAPILog>();
            _fileShareServerManagerMock = new Mock<IFileShareServerManager>();
            _systemStatisticsReporterFake = new SystemStatisticsReporter(_loggerMock.Object);
            _keplerPingReporterFake = new KeplerPingReporter(_helperMock.Object, _loggerMock.Object);
            _pingServiceMock = new Mock<IPingService>();
            _servicesMgrFake = new Mock<IServicesMgr>();
            _servicesMgrFake.Setup(s => s.CreateProxy<IFileShareServerManager>(ExecutionIdentity.System)).Returns(_fileShareServerManagerMock.Object);
            _servicesMgrFake.Setup(s => s.CreateProxy<IPingService>(ExecutionIdentity.System)).Returns(_pingServiceMock.Object);
            _helperMock.Setup(h => h.GetServicesManager()).Returns(_servicesMgrFake.Object);
            _context = new Mock<IEddsDBContext>();
            _databasePingReporterFake = new DatabasePingReporter(_context.Object, _loggerMock.Object);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendSystemDiscUsage()
        {
            // Arrange
            _sut = new SystemHealthReporter(new[] { _systemStatisticsReporterFake });

            // Act
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // Assert
            result.Should().ContainKey(@"SystemDisc_C:\_UsagePercentage");
            result.Should().ContainKey(@"SystemDisc_C:\_FreeSpaceGB");
        }

        [Test]
        public void SystemHealthReporter_ShouldSendCPUUsage()
        {
            // Arrange
            _sut = new SystemHealthReporter(new[] { _systemStatisticsReporterFake });

            // Act
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // Assert
            result.Should().ContainKey("CpuUsageSystem");
        }

        [Test]
        public void SystemHealthReporter_ShouldSendSystemMemoryStatistics()
        {
            // Arrange
            _sut = new SystemHealthReporter(new[] { _systemStatisticsReporterFake });

            // Act
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // Assert
            result.Should().ContainKey("SystemFreeMemoryPercentage");
        }

        [Test]
        public void SystemHealthReporter_ShouldSendKeplerServiceStatusTrue_WhenStatusIsOk()
        {
            // Arrange
            _pingServiceMock.Setup(x => x.Ping()).ReturnsAsync("OK");
            _sut = new SystemHealthReporter(new[] { _keplerPingReporterFake });

            // Act
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsKeplerServiceAccessible", true }
            };
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendKeplerServiceStatusFalse_WhenStatusIsDown()
        {
            // Arrange
            _pingServiceMock.Setup(x => x.Ping()).ReturnsAsync("Down");
            _sut = new SystemHealthReporter(new[] { _keplerPingReporterFake });

            // Act
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsKeplerServiceAccessible", false }
            };
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendKeplerServiceStatusFalse_WhenServiceThrows()
        {
            // Arrange
            _helperMock.Setup(h => h.GetServicesManager()).Throws(_exception);
            _sut = new SystemHealthReporter(new[] { _keplerPingReporterFake });

            // Act
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsKeplerServiceAccessible", false }
            };
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendDatabaseStatusTrue_WhenDatabaseIsOk()
        {
            // Arrange
            DataTable mockDataTable = new DataTable()
            {
                Columns = { new DataColumn() }
            };
            _context.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>())).Returns(mockDataTable);
            _sut = new SystemHealthReporter(new[] { _databasePingReporterFake });

            // Act
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsDatabaseAccessible", true }
            };
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendDatabaseStatusFalse_WhenDatabaseResponseIsNotAsExpected()
        {
            // Arrange
            _context.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>())).Returns(69);
            _sut = new SystemHealthReporter(new[] { _databasePingReporterFake });

            // Act
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsDatabaseAccessible", false }
            };
            result.Should().Contain(expectedResult);
        }

        [Test]
        public void SystemHealthReporter_ShouldSendDatabaseStatusFalse_WhenDatabaseThrows()
        {
            // Arrange
            _context.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<string>())).Throws(_exception);
            _sut = new SystemHealthReporter(new[] { _databasePingReporterFake });

            // Act
            Dictionary<string, object> result = _sut.GetSystemHealthStatisticsAsync().GetAwaiter().GetResult();

            // Assert
            Dictionary<string, object> expectedResult = new Dictionary<string, object>
            {
                { "IsDatabaseAccessible", false }
            };
            result.Should().Contain(expectedResult);
        }
    }
}
