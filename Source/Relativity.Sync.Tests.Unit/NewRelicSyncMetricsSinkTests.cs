using System;
using System.Collections.Generic;
using System.Linq;
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
		public void ItDoesntSendMetricsOnLog()
		{
			NewRelicSyncMetricsSink sink = new NewRelicSyncMetricsSink(_apmClient.Object);
			Metric metric = Metric.TimedOperation("Test", TimeSpan.FromSeconds(1), CommandExecutionStatus.Completed, "foobar");
			sink.Log(metric);

			// Need to specify all arguments when mocking out a method.
			_apmClient.Verify(
				x => x.Log(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()),
				Times.Never);
		}

		[Test]
		public void ItSendsMetricsOnDispose()
		{
			NewRelicSyncMetricsSink sink = new NewRelicSyncMetricsSink(_apmClient.Object);
			Metric[] expectedMetrics = new Metric[] { Metric.TimedOperation("Test", TimeSpan.FromSeconds(1), CommandExecutionStatus.Completed, "foobar") };

			foreach (Metric m in expectedMetrics)
			{
				sink.Log(m);
			}
			sink.Dispose();

			_apmClient.Verify(
				x => x.Log(It.IsAny<string>(), It.Is<Dictionary<string, object>>(d => MatchesMetrics(d, expectedMetrics))),
				Times.Once);
		}

		[Test]
		public void ItSendsMultipleMetricsInPayload()
		{
			NewRelicSyncMetricsSink sink = new NewRelicSyncMetricsSink(_apmClient.Object);
			Metric[] expectedMetrics = new Metric[]
			{
				Metric.TimedOperation("Test1", TimeSpan.FromDays(1), CommandExecutionStatus.Canceled, "foobar"),
				Metric.TimedOperation("Test2", TimeSpan.FromMilliseconds(1), CommandExecutionStatus.Completed, "foobar"),
				Metric.TimedOperation("Test3", TimeSpan.FromSeconds(1), CommandExecutionStatus.Failed, "foobar")
			};

			foreach (Metric m in expectedMetrics)
			{
				sink.Log(m);
			}
			sink.Dispose();

			_apmClient.Verify(
				x => x.Log(It.IsAny<string>(), It.Is<Dictionary<string, object>>(d => MatchesMetrics(d, expectedMetrics))),
				Times.Once);
		}

		private static bool MatchesMetrics(Dictionary<string, object> customData, Metric[] metrics)
		{
			return customData.Count == metrics.Length && metrics.All(m => customData.Values.Count(obj => MatchesMetric(obj, m)) == 1);
		}

		private static bool MatchesMetric(object me, Metric you)
		{
			if (me is Dictionary<string, object>)
			{
				var meAsDict = me as Dictionary<string, object>;
				return
					meAsDict["Name"].Equals(you.Name) &&
					meAsDict["Type"].Equals(you.Type) &&
					meAsDict["ExecutionStatus"].Equals(you.ExecutionStatus) &&
					meAsDict["Value"].Equals(you.Value);
			}

			return false;
		}
	}
}
