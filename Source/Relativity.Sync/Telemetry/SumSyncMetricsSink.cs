using System;
using System.Collections;
using System.Threading.Tasks;
using Relativity.Sync.KeplerFactory;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Telemetry
{
	internal sealed class SumSyncMetricsSink : ISyncMetricsSink
	{
		private readonly ISyncLog _logger;
		private readonly ISourceServiceFactoryForAdmin _servicesManager;
		private readonly IWorkspaceGuidService _workspaceGuidService;
		private readonly SyncJobParameters _syncJobParameters;

		public SumSyncMetricsSink(ISourceServiceFactoryForAdmin servicesManager, ISyncLog logger, IWorkspaceGuidService workspaceGuidService, SyncJobParameters syncJobParameters)
		{
			_logger = logger;
			_workspaceGuidService = workspaceGuidService;
			_syncJobParameters = syncJobParameters;
			_servicesManager = servicesManager;
        }

		public void Send(IMetric metric)
		{
			SendAsync(metric).GetAwaiter().GetResult();
		}

		private async Task SendAsync(IMetric metric)
		{
			using (IMetricsManager metricsManager = await _servicesManager.CreateProxyAsync<IMetricsManager>().ConfigureAwait(false))
			{
				Guid workspaceGuid = await _workspaceGuidService.GetWorkspaceGuidAsync(_syncJobParameters.WorkspaceId).ConfigureAwait(false);
                IEnumerable sumMetrics = metric.GetSumMetrics();

                foreach (SumMetric sumMetric in sumMetrics)
				{
					try
					{
						await SendSumMetricAsync(metricsManager, sumMetric, workspaceGuid).ConfigureAwait(false);
					}
					catch (Exception e)
					{
						_logger.LogError(e, "Logging to SUM failed. The metric with bucket '{bucket}' and Correlation ID '{correlationId}' had value '{value}'.",
							sumMetric.Bucket, sumMetric.CorrelationId, sumMetric.Value);
					}
				}
			}
		}

		private Task SendSumMetricAsync(IMetricsManager metricsManager, SumMetric metric, Guid workspaceGuid)
		{
			switch (metric.Type)
			{
				case MetricType.PointInTimeString:
					return metricsManager.LogPointInTimeStringAsync(metric.Bucket, workspaceGuid, metric.CorrelationId, metric.Value.ToString());
				case MetricType.PointInTimeLong:
					return metricsManager.LogPointInTimeLongAsync(metric.Bucket, workspaceGuid, metric.CorrelationId, (long)metric.Value);
				case MetricType.PointInTimeDouble:
					return metricsManager.LogPointInTimeDoubleAsync(metric.Bucket, workspaceGuid, metric.CorrelationId, (double)metric.Value);
				case MetricType.TimedOperation:
					return metricsManager.LogTimerAsDoubleAsync(metric.Bucket, workspaceGuid, metric.CorrelationId, (double)metric.Value);
				case MetricType.Counter:
					return metricsManager.LogCountAsync(metric.Bucket, workspaceGuid, metric.CorrelationId, 1);
				case MetricType.GaugeOperation:
					return metricsManager.LogGaugeAsync(metric.Bucket, workspaceGuid, metric.CorrelationId, (long)metric.Value);
				default:
					_logger.LogDebug("Logging metric type '{type}' to SUM is not implemented.", metric.Type);
					return Task.CompletedTask;
			}
		}
	}
}