﻿using Moq;
using NUnit.Framework;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry
{
	[TestFixture]
	class SplunkSyncMetricsSinkTests
	{
		private Mock<ISyncLog> _syncLogMock;

		private SplunkSyncMetricsSink _sut;

		[SetUp]
		public void SetUp()
		{
			_syncLogMock = new Mock<ISyncLog>();

			_sut = new SplunkSyncMetricsSink(_syncLogMock.Object);
		}

		[Test]
		public void Send_ShouldSendAggregateMetric()
		{
			// Arrange
			IMetric metric = new TestMetric {TestValue = 1};

			// Act
			_sut.Send(metric);

			// Assert
			_syncLogMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("@")), typeof(TestMetric), metric));
		}

		internal class TestMetric : MetricBase
		{
			public int? TestValue { get; set; }
		}
	}
}
