using Relativity.DataTransfer.MessageService.MetricsManager.APM;

namespace kCura.IntegrationPoints.Common.Monitoring.Messages
{
    public static class MetricMetadataExtensions
    {
        public static T GetValueOrDefault<T>(this IMetricMetadata @this, string key)
        {
            object value;
            return @this.CustomData.TryGetValue(key, out value) ? (T) value : default(T);
        }
    }
}