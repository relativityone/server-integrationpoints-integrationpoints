namespace Relativity.Sync.Telemetry
{
    /// <summary>
    /// Provides methods for logging metrics.
    /// </summary>
    internal interface ISyncMetrics
    {
        /// <summary>
        ///     Send aggregate metrics based on <see cref="IMetric"/> implementation class properties
        /// </summary>
        /// <param name="metric">Metric object</param>
        void Send(IMetric metric);
    }
}