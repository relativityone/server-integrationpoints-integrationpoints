using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class SyncMetricsTests
	{
		private SyncMetrics _instance;

		private Mock<ISyncMetricsSink> _sink1;
		private Mock<ISyncMetricsSink> _sink2;

		private const string _CORRELATION_ID = "id";

		[SetUp]
		public void SetUp()
		{
			_sink1 = new Mock<ISyncMetricsSink>();
			_sink2 = new Mock<ISyncMetricsSink>();

			IEnumerable<ISyncMetricsSink> sinks = new[]
			{
				_sink1.Object,
				_sink2.Object
			};
			_instance = new SyncMetrics(sinks, new CorrelationId(_CORRELATION_ID));
		}

		[Test]
		public void ItShouldSendTimedOperation()
		{
			const string name = "name";
			TimeSpan duration = TimeSpan.FromDays(1);
			ExecutionStatus executionStatus = ExecutionStatus.CompletedWithErrors;

			// ACT
			_instance.TimedOperation(name, duration, executionStatus);

			// ASSERT
			_sink1.Verify(x => x.Log(It.Is<Metric>(m => AssertMetric(m, name, duration, executionStatus, null))));
			_sink2.Verify(x => x.Log(It.Is<Metric>(m => AssertMetric(m, name, duration, executionStatus, null))));
		}

		private bool AssertMetric(Metric metric, string name, TimeSpan duration, ExecutionStatus executionStatus, Dictionary<string, object> customData)
		{
			metric.Name.Should().Be(name);
			metric.CorrelationId.Should().Be(_CORRELATION_ID);
			metric.ExecutionStatus.Should().Be(executionStatus);
			metric.Value.Should().Be(duration.TotalMilliseconds);
			if (customData != null)
			{
				metric.CustomData.Should().Equal(customData);
			}

			return true;
		}

		[Test]
		public void ItShouldSendTimedOperationWithCustomData()
		{
			const string name = "name";
			TimeSpan duration = TimeSpan.FromDays(1);
			ExecutionStatus executionStatus = ExecutionStatus.Failed;
			Dictionary<string, object> customData = new Dictionary<string, object>
			{
				{"key", "value"}
			};

			// ACT
			_instance.TimedOperation(name, duration, executionStatus, customData);

			// ASSERT
			_sink1.Verify(x => x.Log(It.Is<Metric>(m => AssertMetric(m, name, duration, executionStatus, customData))));
			_sink2.Verify(x => x.Log(It.Is<Metric>(m => AssertMetric(m, name, duration, executionStatus, customData))));
		}

		[Test]
		public void ItShouldSendCountOperation()
		{
			const string name = "name";
			ExecutionStatus executionStatus = ExecutionStatus.Canceled;

			// ACT
			_instance.CountOperation(name, executionStatus);

			// ASSERT
			_sink1.Verify(x => x.Log(It.Is<Metric>(m => m.Name == name && m.ExecutionStatus == executionStatus)));
			_sink2.Verify(x => x.Log(It.Is<Metric>(m => m.Name == name && m.ExecutionStatus == executionStatus)));
		}
	}
}