namespace Relativity.Sync.Telemetry
{
    /// <summary>
    ///     Sink for sending <see cref="IMetric"/>s to a specific collector
    /// </summary>
    internal interface ISyncMetricsSink
    {
        /// <summary>
        ///     Send metric for given Sink
        /// </summary>
        /// <param name="metric">Metric to send</param>
        void Send(IMetric metric);
    }
}
