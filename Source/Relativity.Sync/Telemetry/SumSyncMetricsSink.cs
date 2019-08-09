using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Telemetry
{
	internal sealed class SumSyncMetricsSink : ISyncMetricsSink
	{
		private readonly ISyncLog _logger;
		private readonly IServicesMgr _servicesManager;
		private readonly IWorkspaceGuidService _workspaceGuidService;
		private readonly SyncJobParameters _syncJobParameters;

		public SumSyncMetricsSink(IServicesMgr servicesManager, ISyncLog logger, IWorkspaceGuidService workspaceGuidService, SyncJobParameters syncJobParameters)
		{
			_logger = logger;
			_workspaceGuidService = workspaceGuidService;
			_syncJobParameters = syncJobParameters;
			_servicesManager = servicesManager;
		}

		public void Log(Metric metric)
		{
			try
			{
				using (IMetricsManager metricsManager = _servicesManager.CreateProxy<IMetricsManager>(ExecutionIdentity.System))
				{
					LogSumMetric(metricsManager, metric).GetAwaiter().GetResult();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Logging to SUM failed. The metric with bucket '{bucket}' and workflow ID '{workflowId}' had value '{value}'.", metric.Name, metric.WorkflowId, metric.Value);
			}
		}

		private async Task LogSumMetric(IMetricsManager metricsManager, Metric metric)
		{
			Guid workspaceGuid = await _workspaceGuidService.GetWorkspaceGuidAsync(_syncJobParameters.WorkspaceId).ConfigureAwait(false);

			MetricType metricType = metric.Type;
			switch (metricType)
			{
				case MetricType.PointInTimeString:
					await LogPointInTimeString(metricsManager, metric, workspaceGuid).ConfigureAwait(false);
					break;
				case MetricType.PointInTimeLong:
					await LogPointInTimeLong(metricsManager, metric, workspaceGuid).ConfigureAwait(false);
					break;
				case MetricType.PointInTimeDouble:
					await LogPointInTimeDouble(metricsManager, metric, workspaceGuid).ConfigureAwait(false);
					break;
				case MetricType.TimedOperation:
					await LogTimedOperation(metricsManager, metric, workspaceGuid).ConfigureAwait(false);
					break;
				case MetricType.Counter:
					await LogCounterOperation(metricsManager, metric, workspaceGuid).ConfigureAwait(false);
					break;
				case MetricType.GaugeOperation:
					await LogGaugeOperation(metricsManager, metric, workspaceGuid).ConfigureAwait(false);
					break;
				default:
					_logger.LogDebug("Logging metric type '{type}' to SUM is not implemented.", metricType);
					break;
			}
		}

		private static async Task LogPointInTimeString(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			await metricsManager.LogPointInTimeStringAsync(metric.Name, workspaceGuid, metric.WorkflowId, metric.Value.ToString()).ConfigureAwait(false);
		}

		private static async Task LogPointInTimeLong(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			await metricsManager.LogPointInTimeLongAsync(metric.Name, workspaceGuid, metric.WorkflowId, (long)metric.Value).ConfigureAwait(false);
		}

		private static async Task LogPointInTimeDouble(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			await metricsManager.LogPointInTimeDoubleAsync(metric.Name, workspaceGuid, metric.WorkflowId, (double)metric.Value).ConfigureAwait(false);
		}

		private static async Task LogGaugeOperation(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			await metricsManager.LogGaugeAsync(metric.Name, workspaceGuid, metric.WorkflowId, (long)metric.Value).ConfigureAwait(false);
		}

		private static async Task LogCounterOperation(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			await metricsManager.LogCountAsync(metric.Name, workspaceGuid, metric.WorkflowId, 1).ConfigureAwait(false);
		}

		private static async Task LogTimedOperation(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			await metricsManager.LogTimerAsDoubleAsync(metric.Name, workspaceGuid, metric.WorkflowId, (double)metric.Value).ConfigureAwait(false);
		}
	}
}