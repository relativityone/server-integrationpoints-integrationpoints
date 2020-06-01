using kCura.ScheduleQueue.Core;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.Metrics
{
	public class SyncJobMetric : ISyncJobMetric
	{
		private readonly IMetricsFactory _metricsFactory;

		public SyncJobMetric(IMetricsFactory metricsFactory)
		{
			_metricsFactory = metricsFactory;
		}

		public async Task SendJobStartedAsync(Job job)
		{
			IMetricCollection metrics = new MetricsCollection()
				.AddMetric(_metricsFactory.CreateScheduleJobStartedMetric(job));

			await metrics.SendAsync().ConfigureAwait(false);
		}

		public async Task SendJobCompletedAsync(Job job)
		{
			IMetricCollection metrics = new MetricsCollection()
				.AddMetric(_metricsFactory.CreateScheduleJobCompletedMetric(job));

			await metrics.SendAsync().ConfigureAwait(false);
		}

		public async Task SendJobFailedAsync(Job job)
		{
			IMetricCollection metrics = new MetricsCollection()
				.AddMetric(_metricsFactory.CreateScheduleJobFailedMetric(job));

			await metrics.SendAsync().ConfigureAwait(false);
		}

		public IDisposable SendJobDuration()
		{
			return null;
			//_metricsFactory.
		}
}
}
