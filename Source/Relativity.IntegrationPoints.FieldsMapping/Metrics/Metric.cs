using System;
using System.Collections.Generic;

namespace Relativity.IntegrationPoints.FieldsMapping.Metrics
{
    public class Metric
    {
        private IDictionary<string, object> _customData;
        private Metric(string name, MetricType type, string workflowId)
        {
            Name = name;
            Type = type;
            WorkflowId = workflowId;
        }

        /// <summary>
        /// Name of this metric.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Type of metric this represents, e.g. a timed operation, a counter, etc.
        /// </summary>
        public MetricType Type { get; }

        /// <summary>
        /// ID that correlates the metric to a particular system usage workflow.
        /// </summary>
        public string WorkflowId { get; set; }

        /// <summary>
        /// Value of this metric.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Any custom data associated with this metric.
        /// </summary>
        public IDictionary<string, object> CustomData
        {
            get => _customData ?? (_customData = new Dictionary<string, object>());

            set => _customData = value;
        }

        public static Metric CountOperation(string name, string workflowId)
        {
            return new Metric(name, MetricType.Counter, workflowId);
        }

        public static Metric GaugeOperation(string name, string workflowId, long value, string unitOfMeasure)
        {
            return new Metric(name, MetricType.GaugeOperation, workflowId)
            {
                Value = value,
                CustomData = new Dictionary<string, object>() { { "unitOfMeasure", unitOfMeasure } }
            };
        }

        public static Metric TimedOperation(string name, TimeSpan duration, string workflowId)
        {
            return new Metric(name, MetricType.TimedOperation, workflowId)
            {
                Value = duration.TotalMilliseconds
            };
        }
    }
}
