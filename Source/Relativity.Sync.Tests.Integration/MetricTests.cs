using System;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class MetricTests
	{
		private Metric _metric;

		[SetUp]
		public void SetUp()
		{
			_metric = Metric.TimedOperation("metric name", TimeSpan.FromMilliseconds(1), CommandExecutionStatus.Completed, "correlation ID");
		}

		[Test]
		public void ItShouldSerializeMetricWithoutCustomData()
		{
			// ACT
			string serializedObject = JsonConvert.SerializeObject(_metric);

			// ASSERT
			serializedObject.Should().NotBeNullOrWhiteSpace();
		}

		[Test]
		public void ItShouldSerializeMetricWithCustomData()
		{
			_metric.CustomData.Add("custom data int", 1);
			_metric.CustomData.Add("custom data object", new object());
			_metric.CustomData.Add("custom data null", null);

			// ACT
			string serializedObject = JsonConvert.SerializeObject(_metric);

			// ASSERT
			serializedObject.Should().NotBeNullOrWhiteSpace();
		}
	}
}