using kCura.ScheduleQueue.Core;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.Metrics
{
	public class SyncJobMetric : ISyncJobMetric
	{
		private readonly IMetricsFactory _metricsFactory;
		private readonly IAPILog _logger;

		public SyncJobMetric(IMetricsFactory metricsFactory, IAPILog logger)
		{
			_metricsFactory = metricsFactory;
			_logger = logger;
		}

		public async Task SendJobStartedAsync(Job job)
		{
			try
			{
				IMetricCollection metrics = new MetricsCollection()
					.AddMetric(_metricsFactory.CreateScheduleJobStartedMetric(job));

				await metrics.SendAsync().ConfigureAwait(false);
			}
			catch(Exception ex)
			{
				const string errorMsg = "Send metrics failed on job started";
				_logger.LogError(ex, errorMsg);
				throw new SyncMetricException(errorMsg, ex);
			}
		}

		public async Task SendJobCompletedAsync(Job job)
		{
			try
			{
				IMetricCollection metrics = new MetricsCollection()
					.AddMetric(_metricsFactory.CreateScheduleJobCompletedMetric(job));

				await metrics.SendAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				const string errorMsg = "Send metrics failed on job completed";
				_logger.LogError(ex, errorMsg);
				throw new SyncMetricException(errorMsg, ex);
			}
		}

		public async Task SendJobFailedAsync(Job job)
		{
			try
			{
				IMetricCollection metrics = new MetricsCollection()
					.AddMetric(_metricsFactory.CreateScheduleJobFailedMetric(job));

				await metrics.SendAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				const string errorMsg = "Send metrics failed on job failed";
				_logger.LogError(ex, errorMsg);
				throw new SyncMetricException(errorMsg, ex);
			}
		}
	}
}
