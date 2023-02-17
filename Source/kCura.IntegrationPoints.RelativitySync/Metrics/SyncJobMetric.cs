using kCura.ScheduleQueue.Core;
using Relativity.API;
using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;

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
                IMetric metric = _metricsFactory.CreateScheduleJobStartedMetric(job);
                _logger.LogInformation("Sending Sync job started metric: {@metric}", metric);

                IMetricCollection metrics = new MetricsCollection().AddMetric(metric);

                await metrics.SendAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
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
                IMetric metric = _metricsFactory.CreateScheduleJobCompletedMetric(job);

                if (!(metric is EmptyMetric))
                {
                    _logger.LogInformation("Sending Sync job completed metric: {@metric}", metric);
                }

                IMetricCollection metrics = new MetricsCollection().AddMetric(metric);

                await metrics.SendAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                const string errorMsg = "Send metrics failed on job completed";
                _logger.LogError(ex, errorMsg);
                throw new SyncMetricException(errorMsg, ex);
            }
        }

        public async Task SendJobFailedAsync(Job job, Exception e)
        {
            try
            {
                IMetric metric = _metricsFactory.CreateScheduleJobFailedMetric(job);
                _logger.LogInformation("Sending Sync job failed metric: {@metric}", metric);
                _logger.LogError(e, "Sending Sync job failed metric: {@metric} with exception.", metric);

                IMetricCollection metrics = new MetricsCollection().AddMetric(metric);

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
