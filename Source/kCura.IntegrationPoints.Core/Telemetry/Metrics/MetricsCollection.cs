using Relativity.API;
using Relativity.Telemetry.Services.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Telemetry.Metrics
{
	public class MetricsCollection : IMetricCollection
	{
		private readonly IList<IMetric> _metrics = new List<IMetric>();

		private readonly IServicesMgr _servicesMgr;

		public MetricsCollection(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

		public async Task SendAsync()
		{
			using (var metricsManager = _servicesMgr.CreateProxy<IMetricsManager>(ExecutionIdentity.System))
			{
				foreach (var metric in _metrics)
				{
					if (metric.CanSend())
					{
						await metric.SendAsync(metricsManager).ConfigureAwait(false);
					}
				}
			}
		}

		public IMetricCollection AddMetric<T>(T metric) where T : IMetric
		{
			_metrics.Add(metric);

			return this;
		}
	}
}
