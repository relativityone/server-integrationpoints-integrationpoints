using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit.Telemetry
{
	[TestFixture]
	public class MetricTests
	{
		[Test]
		public void Test()
		{
			// Act
			Metric metric = Metric.PointInTimeLongOperation("test", 1000, "sync-test");

			metric.CustomData = new Dictionary<string, object>()
			{
				{"aaa", 1},
				{"bbb", new StringBuilder()},
				{"ccc", ExecutionStatus.Completed}
			};

			// Assert
			var customData = metric.ToDictionary();
		}
	}
}
