using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Common.Metrics.Sink;

namespace kCura.IntegrationPoints.Common.Metrics
{
    public class RipMetrics : IRipMetrics
    {
        private readonly IEnumerable<IRipMetricsSink> _sinks;

        public RipMetrics(IEnumerable<IRipMetricsSink> sinks)
        {
            _sinks = sinks;
        }

        public void PointInTimeDouble(string name, double value, Dictionary<string, object> customData, string correlationId)
        {
            foreach (IRipMetricsSink sink in _sinks)
            {
                RipMetric metric = RipMetric.PointInTimeDouble(name, value, correlationId);
                metric.CustomData = customData;

                sink.Log(metric);
            }
        }

        public void PointInTimeLong(string name, long value, Dictionary<string, object> customData, string correlationId)
        {
            foreach (IRipMetricsSink sink in _sinks)
            {
                RipMetric metric = RipMetric.PointInTimeLong(name, value, correlationId);
                metric.CustomData = customData;

                sink.Log(metric);
            }
        }

        public void TimedOperation(string name, TimeSpan duration, Dictionary<string, object> customData, string correlationId)
        {
            foreach (IRipMetricsSink sink in _sinks)
            {
                RipMetric metric = RipMetric.TimedOperation(name, duration, correlationId);
                metric.CustomData = customData;

                sink.Log(metric);
            }
        }
    }
}
