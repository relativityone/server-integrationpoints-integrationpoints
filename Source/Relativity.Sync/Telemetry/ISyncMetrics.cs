using Relativity.Sync.Telemetry.RelEye;

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

        /// <summary>
        ///     Send aggregate events based on <see cref="IEvent"/> implementation class properties
        /// </summary>
        /// <param name="event"></param>
        void Send(IEvent @event);
    }
}
