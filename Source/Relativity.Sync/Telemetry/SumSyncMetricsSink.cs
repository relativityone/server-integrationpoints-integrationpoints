using System;
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
					LogSumMetric(metricsManager, metric);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Logging to SUM failed. The metric with bucket '{bucket}' and workflow ID '{workflowId}' had value '{value}'.", metric.Name, metric.WorkflowId, metric.Value);
			}
		}

		private void LogSumMetric(IMetricsManager metricsManager, Metric metric)
		{
			Guid workspaceGuid = _workspaceGuidService.GetWorkspaceGuidAsync(_syncJobParameters.WorkspaceId).ConfigureAwait(false).GetAwaiter().GetResult();

			MetricType metricType = metric.Type;
			switch (metricType)
			{
				case MetricType.PointInTimeString:
					LogPointInTimeString(metricsManager, metric, workspaceGuid);
					break;
				case MetricType.PointInTimeLong:
					LogPointInTimeLong(metricsManager, metric, workspaceGuid);
					break;
				case MetricType.PointInTimeDouble:
					LogPointInTimeDouble(metricsManager, metric, workspaceGuid);
					break;
				case MetricType.TimedOperation:
					LogTimedOperation(metricsManager, metric, workspaceGuid);
					break;
				case MetricType.Counter:
					LogCounterOperation(metricsManager, metric, workspaceGuid);
					break;
				case MetricType.GaugeOperation:
					LogGaugeOperation(metricsManager, metric, workspaceGuid);
					break;
				default:
					_logger.LogDebug("Logging metric type '{type}' to SUM is not implemented.", metricType);
					break;
			}
		}

		private static void LogPointInTimeString(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			metricsManager.LogPointInTimeStringAsync(metric.Name, workspaceGuid, metric.WorkflowId, metric.Value.ToString());
		}

		private static void LogPointInTimeLong(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			metricsManager.LogPointInTimeLongAsync(metric.Name, workspaceGuid, metric.WorkflowId, (long)metric.Value);
		}

		private static void LogPointInTimeDouble(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			metricsManager.LogPointInTimeDoubleAsync(metric.Name, workspaceGuid, metric.WorkflowId, (double)metric.Value);
		}

		private static void LogGaugeOperation(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			metricsManager.LogGaugeAsync(metric.Name, workspaceGuid, metric.WorkflowId, (long)metric.Value);
		}

		private static void LogCounterOperation(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			metricsManager.LogCountAsync(metric.Name, workspaceGuid, metric.WorkflowId, 1);
		}

		private static void LogTimedOperation(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			metricsManager.LogTimerAsDoubleAsync(metric.Name, workspaceGuid, metric.WorkflowId, (double)metric.Value);
		}
	}
}