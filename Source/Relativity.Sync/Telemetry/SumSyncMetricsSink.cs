using System;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Telemetry
{
	internal sealed class SumSyncMetricsSink : ISyncMetricsSink
	{
		private readonly ISyncLog _logger;
		private readonly IServicesMgr _servicesManager;
		private readonly WorkspaceGuid _workspaceGuid;

		public SumSyncMetricsSink(IServicesMgr servicesManager, ISyncLog logger, WorkspaceGuid workspaceGuid)
		{
			_logger = logger;
			_workspaceGuid = workspaceGuid;
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
			MetricType metricType = metric.Type;
			switch (metricType)
			{
				case MetricType.PointInTimeString:
					LogPointInTimeString(metricsManager, metric);
					break;
				case MetricType.PointInTimeLong:
					LogPointInTimeLong(metricsManager, metric);
					break;
				case MetricType.PointInTimeDouble:
					LogPointInTimeDouble(metricsManager, metric);
					break;
				case MetricType.TimedOperation:
					LogTimedOperation(metricsManager, metric);
					break;
				case MetricType.Counter:
					LogCounterOperation(metricsManager, metric);
					break;
				case MetricType.GaugeOperation:
					LogGaugeOperation(metricsManager, metric);
					break;
				default:
					_logger.LogDebug("Logging metric type '{type}' to SUM is not implemented.", metricType);
					break;
			}
		}

		private void LogPointInTimeString(IMetricsManager metricsManager, Metric metric)
		{
			metricsManager.LogPointInTimeStringAsync(metric.Name, _workspaceGuid.Value, metric.WorkflowId, metric.Value.ToString());
		}

		private void LogPointInTimeLong(IMetricsManager metricsManager, Metric metric)
		{
			metricsManager.LogPointInTimeLongAsync(metric.Name, _workspaceGuid.Value, metric.WorkflowId, (long)metric.Value);
		}

		private void LogPointInTimeDouble(IMetricsManager metricsManager, Metric metric)
		{
			metricsManager.LogPointInTimeDoubleAsync(metric.Name, _workspaceGuid.Value, metric.WorkflowId, (double)metric.Value);
		}

		private void LogGaugeOperation(IMetricsManager metricsManager, Metric metric)
		{
			metricsManager.LogGaugeAsync(metric.Name, _workspaceGuid.Value, metric.WorkflowId, (long)metric.Value);
		}

		private void LogCounterOperation(IMetricsManager metricsManager, Metric metric)
		{
			metricsManager.LogCountAsync(metric.Name, _workspaceGuid.Value, metric.WorkflowId, 1);
		}

		private void LogTimedOperation(IMetricsManager metricsManager, Metric metric)
		{
			metricsManager.LogTimerAsDoubleAsync(metric.Name, _workspaceGuid.Value, metric.WorkflowId, (double)metric.Value);
		}
	}
}