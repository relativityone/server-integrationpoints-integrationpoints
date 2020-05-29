using kCura.ScheduleQueue.Core;
using Relativity.API;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Telemetry.Metrics
{
	public class JobMetric : IJobMetric
	{
		private readonly IServicesMgr _servicesMgr;
		private readonly IMetricsFactory _metricsFactory;

		public JobMetric(IServicesMgr servicesMgr, IMetricsFactory metricsFactory)
		{
			_servicesMgr = servicesMgr;
			_metricsFactory = metricsFactory;
		}

		public async Task SendJobStartedAsync(Job job)
		{
			IMetricCollection metrics = new MetricsCollection(_servicesMgr)
				.AddMetric(_metricsFactory.CreateScheduleJobStartedMetric(job));

			await metrics.SendAsync().ConfigureAwait(false);
		}

		public async Task SendJobCompletedAsync(Job job)
		{
			IMetricCollection metrics = new MetricsCollection(_servicesMgr)
				.AddMetric(_metricsFactory.CreateScheduleJobCompletedMetric(job));

			await metrics.SendAsync().ConfigureAwait(false);
		}

		public async Task SendJobFailedAsync(Job job)
		{
			IMetricCollection metrics = new MetricsCollection(_servicesMgr)
				.AddMetric(_metricsFactory.CreateScheduleJobFailedMetric(job));

			await metrics.SendAsync().ConfigureAwait(false);
		}
}
}
