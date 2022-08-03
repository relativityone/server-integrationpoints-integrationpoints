using Relativity.API;
using Relativity.Telemetry.Services.Metrics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.Metrics
{
    public class MetricsCollection : IMetricCollection
    {
        private readonly IList<IMetric> _metrics = new List<IMetric>();

        public async Task SendAsync()
        {
            foreach (var metric in _metrics)
            {
                await metric.SendAsync().ConfigureAwait(false);
            }
        }

        public IMetricCollection AddMetric<T>(T metric) where T : IMetric
        {
            _metrics.Add(metric);

            return this;
        }
    }
}
