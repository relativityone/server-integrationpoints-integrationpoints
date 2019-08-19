using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class NewRelicSyncMetricsSinkTests
	{
		private Mock<IAPMClient> _apmClient;

		[SetUp]
		public void SetUp()
		{
			_apmClient = new Mock<IAPMClient>(MockBehavior.Loose);
		}

		[Test]
		public void ItSendsMetricsOnLog()
		{
			NewRelicSyncMetricsSink sink = new NewRelicSyncMetricsSink(_apmClient.Object);
			Metric metric = Metric.TimedOperation("Test", TimeSpan.FromSeconds(1), ExecutionStatus.Completed, "foobar");
			sink.Log(metric);

			_apmClient.Verify(x => x.Log(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
		}

		[Test]
		public void ItSendsMetricInDictionaryThatIsSerializableByPeriodicBatchingSink()
		{
			NewRelicSyncMetricsSink sink = new NewRelicSyncMetricsSink(_apmClient.Object);
			Metric[] expectedMetrics = { Metric.TimedOperation("Test", TimeSpan.FromSeconds(1), ExecutionStatus.Completed, "foobar") };

			foreach (Metric m in expectedMetrics)
			{
				sink.Log(m);
			}

			_apmClient.Verify(
				x => x.Log(It.IsAny<string>(), It.Is<Dictionary<string, object>>(d => DictionaryIsSerializableByPeriodicBatchingSink(d))),
				Times.Exactly(expectedMetrics.Length));
		}

		private bool DictionaryIsSerializableByPeriodicBatchingSink(Dictionary<string, object> metricDictionary)
		{
			var periodicBatchingSinkStub = new PeriodicBatchingSinkStub();

			string serializedMetric = periodicBatchingSinkStub.ToJson(metricDictionary);

			return !string.IsNullOrEmpty(serializedMetric);
		}
	}
}
