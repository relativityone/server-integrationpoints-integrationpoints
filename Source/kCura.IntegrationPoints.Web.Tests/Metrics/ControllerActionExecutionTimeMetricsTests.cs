using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Common.Metrics;
using kCura.IntegrationPoints.Web.Metrics;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Metrics
{
    [TestFixture]
    public class ControllerActionExecutionTimeMetricsTests
    {
        private Mock<IDateTimeHelper> _dateTimeFake;
        private Mock<IRipMetrics> _ripMetricsMock;

        [SetUp]
        public void SetUp()
        {
            _dateTimeFake = new Mock<IDateTimeHelper>();
            _ripMetricsMock = new Mock<IRipMetrics>();
        }

        [Test]
        public void LogExecutionTime_ShouldReportTimedOperation()
        {
            // Arrange
            const string metricName = "IntegrationPoint.CustomPage.ResponseTime";
            DateTime startTime = new DateTime(2020, 10, 1, 10, 0, 0);
            DateTime endTime = startTime.AddSeconds(1);
            TimeSpan expectedDuration = endTime - startTime;
            _dateTimeFake.Setup(x => x.Now()).Returns(endTime);
            const string url = "/fake/url";
            const string method = "POST";
            const string correlationId = "dcf6e9d1-22b6-4da3-98f6-41381e93c30c";
            ControllerActionExecutionTimeMetrics sut = new ControllerActionExecutionTimeMetrics(_dateTimeFake.Object, _ripMetricsMock.Object);

            // Act
            sut.LogExecutionTime(url, startTime, method, correlationId);

            // Assert
            _ripMetricsMock.Verify(x => x.TimedOperation(
                It.Is<string>(name => name == metricName),
                It.Is<TimeSpan>(time => time == expectedDuration),
                It.Is<Dictionary<string, object>>(dictionary => dictionary.ContainsKey("ActionURL") &&
                                                                dictionary["ActionURL"].ToString() == url &&
                                                                dictionary.ContainsKey("ResponseTimeMs") &&
                                                                (long)dictionary["ResponseTimeMs"] == (long)expectedDuration.TotalMilliseconds &&
                                                                dictionary.ContainsKey("Method") &&
                                                                dictionary["Method"].ToString() == method),
                correlationId));
        }
    }
}
