using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Relativity.Sync.Telemetry.Metrics
{
    internal abstract class MetricBase<T> : IMetric
        where T : IMetric, new()
    {
        private static Dictionary<PropertyInfo, MetricAttribute> _metricCacheProperties;
        
        [Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.JOB_CORRELATION_ID)]
        public string CorrelationId { get; set; }

        public string Name { get; }
        
        public string ExecutingApplication { get; set;  }
        
        public string ExecutingApplicationVersion { get; set; }

        public string SyncVersion { get; set; }

        public string DataSourceType { get; set; }

        public string DataDestinationType { get; set; }

        public string FlowName { get; set; }

        public bool IsRetry { get; set; }

        protected MetricBase()
        {
            Name = GetType().Name;
        }

        protected MetricBase(string metricName)
        {
            Name = metricName;
        }

        public virtual Dictionary<string, object> GetApmMetrics()
        {
            Dictionary<PropertyInfo, MetricAttribute> metricProperties = GetMetricProperties();
            Dictionary<string, object> apmMetrics = metricProperties
                .Where(item => item.Key.GetCustomAttribute<APMIgnoreMetricAttribute>() == null)
                .ToDictionary(keyValuePair => keyValuePair.Key.Name, keyValuePair => GetValue(keyValuePair.Key));
            
            return apmMetrics;
        }

        public virtual IEnumerable<SumMetric> GetSumMetrics()
        {
            Dictionary<PropertyInfo, MetricAttribute> metricProperties = GetMetricProperties();
            return metricProperties
                .Where(p => p.Value != null)
                .Select(p => new SumMetric
                {
                    CorrelationId = CorrelationId,
                    Type = p.Value.Type,
                    Bucket = GetBucketName(p.Value),
                    Value = p.Key.GetValue(this)
                })
                .Where(m => m.Value != null);
        }

        protected virtual string GetBucketName(MetricAttribute attr) => attr.Name ?? Name;

        private object GetValue(PropertyInfo p)
        {
            return Nullable.GetUnderlyingType(p.PropertyType)?.IsEnum == true ? p.GetValue(this)?.ToString() : p.GetValue(this);
        }

        private Dictionary<PropertyInfo, MetricAttribute> GetMetricProperties()
        {
            return _metricCacheProperties ??
                   (_metricCacheProperties = GetType()
                       .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                       .Where(p => p.GetMethod != null)
                       .ToDictionary(p => p, p => p.GetCustomAttribute<MetricAttribute>()));
        }

    }
}
