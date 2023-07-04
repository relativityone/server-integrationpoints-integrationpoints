using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Common.Metrics
{
    public class RipMetric
    {
        private Dictionary<string, object> _customData;

        private RipMetric(string name, RipMetricType type, string correlationId)
        {
            Name = name;
            Type = type;
            CorrelationId = correlationId;
        }

        /// <summary>
        /// Name of this metric.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Type of metric this represents, e.g. a timed operation, a counter, etc.
        /// </summary>
        public RipMetricType Type { get; }

        /// <summary>
        /// ID that correlates the metric to a particular system usage workflow.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Value of this metric.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Any custom data associated with this metric.
        /// </summary>
        public Dictionary<string, object> CustomData
        {
            get
            {
                return _customData ??
                    (_customData = new Dictionary<string, object>()
                    {
                        { "r1.team.id", "PTCI-2456712" },
                        { "service.name", "integrationpoints-repo" },
                    });
            }

            set
            {
                _customData = value;
            }
        }

        public static RipMetric TimedOperation(string name, TimeSpan duration, string correlationId)
        {
            return new RipMetric(name, RipMetricType.TimedOperation, correlationId)
            {
                Value = duration.TotalMilliseconds
            };
        }

        public static RipMetric PointInTimeLong(string name, long value, string correlationId)
        {
            return new RipMetric(name, RipMetricType.PointInTimeLong, correlationId)
            {
                Value = value
            };
        }

        public static RipMetric PointInTimeDouble(string name, double value, string correlationId)
        {
            return new RipMetric(name, RipMetricType.PointInTimeDouble, correlationId)
            {
                Value = value
            };
        }
    }
}
