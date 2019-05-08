using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Telemetry;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class SyncMetricsTests
	{
		private SyncMetrics _instance;

		private Mock<ISyncMetricsSink> _sink1;
		private Mock<ISyncMetricsSink> _sink2;

		private const string _CORRELATION_ID = "id";
		private const string _NAME = "name";

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
			TimeSpan duration = TimeSpan.FromDays(1);
			ExecutionStatus executionStatus = ExecutionStatus.CompletedWithErrors;

			// ACT
			_instance.TimedOperation(_NAME, duration, executionStatus);

			// ASSERT
			_sink1.Verify(x => x.Log(It.Is<Metric>(m => AssertTimedOperationMetric(m, duration, executionStatus, null))));
			_sink2.Verify(x => x.Log(It.Is<Metric>(m => AssertTimedOperationMetric(m, duration, executionStatus, null))));
		}

		[Test]
		public void ItShouldSendTimedOperationWithCustomData()
		{
			TimeSpan duration = TimeSpan.FromDays(1);
			ExecutionStatus executionStatus = ExecutionStatus.Failed;
			Dictionary<string, object> customData = new Dictionary<string, object>
			{
				{"key", "value"}
			};

			// ACT
			_instance.TimedOperation(_NAME, duration, executionStatus, customData);

			// ASSERT
			_sink1.Verify(x => x.Log(It.Is<Metric>(m => AssertTimedOperationMetric(m, duration, executionStatus, customData))));
			_sink2.Verify(x => x.Log(It.Is<Metric>(m => AssertTimedOperationMetric(m, duration, executionStatus, customData))));
		}

		[Test]
		public void ItShouldSendCountOperation()
		{
			ExecutionStatus executionStatus = ExecutionStatus.Canceled;

			// ACT
			_instance.CountOperation(_NAME, executionStatus);

			// ASSERT
			_sink1.Verify(x => x.Log(It.Is<Metric>(m => m.Name == _NAME && m.ExecutionStatus == executionStatus)));
			_sink2.Verify(x => x.Log(It.Is<Metric>(m => m.Name == _NAME && m.ExecutionStatus == executionStatus)));
		}

		[Test]
		public void ItShouldSendTimedOperationUsingDisposableStopwatch()
		{
			ExecutionStatus executionStatus = ExecutionStatus.None;
			var expectedCustomData = new Dictionary<string, object>();

			// ACT
			using (_instance.TimedOperation(_NAME, executionStatus))
			{
			}

			// ASSERT
			_sink1.Verify(x => x.Log(It.Is<Metric>(m => AssertMetric(m, MetricType.TimedOperation, ExecutionStatus.None, expectedCustomData) && m.Value != null)));
			_sink2.Verify(x => x.Log(It.Is<Metric>(m => AssertMetric(m, MetricType.TimedOperation, ExecutionStatus.None, expectedCustomData) && m.Value != null)));
		}

		[Test]
		public void ItShouldSendGaugeOperation()
		{
			ExecutionStatus executionStatus = ExecutionStatus.None;
			const int gaugeValue = 12345;
			const string unitOfMeasure = "docs";
			const int allDocs = 98765;

			var customData = new Dictionary<string, object>()
			{
				{ "allDocs", allDocs }
			};

			// ACT
			_instance.GaugeOperation(_NAME, executionStatus, gaugeValue, unitOfMeasure, customData);

			// ASSERT
			_sink1.Verify(x => x.Log(It.Is<Metric>(m => AssertGaugeOperationMetric(m, gaugeValue, unitOfMeasure, executionStatus, customData))));
			_sink2.Verify(x => x.Log(It.Is<Metric>(m => AssertGaugeOperationMetric(m, gaugeValue, unitOfMeasure, executionStatus, customData))));
		}

		private static bool AssertMetric(Metric metric, MetricType metricType, ExecutionStatus executionStatus, Dictionary<string, object> customData)
		{
			metric.Type.Should().Be(metricType);
			metric.Name.Should().Be(_NAME);
			metric.CorrelationId.Should().Be(_CORRELATION_ID);
			metric.ExecutionStatus.Should().Be(executionStatus);

			if (customData != null)
			{
				metric.CustomData.Should().Equal(customData);
			}

			return true;
		}

		private static bool AssertTimedOperationMetric(Metric metric, TimeSpan duration, ExecutionStatus executionStatus, Dictionary<string, object> customData)
		{
			AssertMetric(metric, MetricType.TimedOperation, executionStatus, customData);

			metric.Value.Should().Be(duration.TotalMilliseconds);

			return true;
		}

		private static bool AssertGaugeOperationMetric(Metric metric, long gaugeValue, string unitOfMeasure, ExecutionStatus executionStatus, Dictionary<string, object> customData)
		{
			const string unitOfMeasureKey = "unitOfMeasure";
			if (customData.ContainsKey(unitOfMeasureKey))
			{
				customData[unitOfMeasureKey].Should().Be(unitOfMeasure);
			}
			else
			{
				customData.Add(unitOfMeasureKey, unitOfMeasure);
			}

			AssertMetric(metric, MetricType.GaugeOperation, executionStatus, customData);

			metric.Value.Should().Be(gaugeValue);

			return true;
		}
	}
}