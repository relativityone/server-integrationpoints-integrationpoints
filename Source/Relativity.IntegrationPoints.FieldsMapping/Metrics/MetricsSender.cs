using System;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.FieldsMapping.Metrics
{
    public class MetricsSender : IMetricsSender
    {
        private readonly IEnumerable<IMetricsSink> _sinks;
        private readonly Guid _workflowId = Guid.NewGuid();

        public MetricsSender(IEnumerable<IMetricsSink> sinks)
        {
            _sinks = sinks;
        }

        public void CountOperation(string name)
        {
            Metric metric = Metric.CountOperation(name, _workflowId.ToString());

            foreach (IMetricsSink sink in _sinks)
            {
                sink.Log(metric);
            }
        }

        public void GaugeOperation(string name, long value, string unitOfMeasure, Dictionary<string, object> customData = null)
        {
            Metric metric = Metric.GaugeOperation(name, _workflowId.ToString(), value, unitOfMeasure);

            if (customData != null)
            {
                foreach (KeyValuePair<string, object> keyValuePair in customData)
                {
                    metric.CustomData.Add(keyValuePair);
                }
            }

            foreach (IMetricsSink sink in _sinks)
            {
                sink.Log(metric);
            }
        }
    }
}