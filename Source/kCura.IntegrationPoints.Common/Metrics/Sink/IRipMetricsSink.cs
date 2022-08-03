namespace kCura.IntegrationPoints.Common.Metrics.Sink
{
    public interface IRipMetricsSink
    {
        void Log(RipMetric ripMetric);
    }
}