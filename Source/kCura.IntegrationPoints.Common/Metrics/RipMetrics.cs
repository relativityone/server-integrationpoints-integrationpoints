using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Common.Metrics.Sink;

namespace kCura.IntegrationPoints.Common.Metrics
{
	public class RipMetrics : IRipMetrics
	{
		private readonly IEnumerable<IRipMetricsSink> _sinks;
		private readonly Guid _workflowId = Guid.NewGuid();

		public RipMetrics(IEnumerable<IRipMetricsSink> sinks)
		{
			_sinks = sinks;
		}

		public void TimedOperation(string name, TimeSpan duration, Dictionary<string, object> customData)
		{
			foreach (IRipMetricsSink sink in _sinks)
			{
				RipMetric metric = RipMetric.TimedOperation(name, duration, _workflowId.ToString());
				metric.CustomData = customData;

				sink.Log(metric);
			}
		}
	}
}