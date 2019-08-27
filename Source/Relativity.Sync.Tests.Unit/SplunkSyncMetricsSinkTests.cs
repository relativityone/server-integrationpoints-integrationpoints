using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class SplunkSyncMetricsSinkTests
	{
		private Mock<ISyncLog> _logger;

		private SplunkSyncMetricsSink _sut;

		[SetUp]
		public void SetUp()
		{
			_logger = new Mock<ISyncLog>();

			_sut = new SplunkSyncMetricsSink(_logger.Object);
		}

		[Test]
		public void ItShouldLogMetricWithValidParameters()
		{
			const string metricName = "metricName";
			TimeSpan duration = TimeSpan.MaxValue;
			ExecutionStatus executionStatus = ExecutionStatus.Completed;
			const string correlationId = "correlationId";
			
			// act
			Metric metric = Metric.TimedOperation(metricName, duration, executionStatus, correlationId);
			_sut.Log(metric);

			// assert
			_logger.Verify(x => x.LogInformation(It.IsAny<string>(), It.Is<object[]>(objects => objects.Contains(metric))));
		}
	}
}