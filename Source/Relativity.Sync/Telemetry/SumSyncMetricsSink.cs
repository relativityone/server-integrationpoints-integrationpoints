﻿using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Telemetry
{
	internal sealed class SumSyncMetricsSink : ISyncMetricsSink
	{
		private readonly ISyncLog _logger;
		private readonly ISyncServiceManager _servicesManager;
		private readonly IWorkspaceGuidService _workspaceGuidService;
		private readonly SyncJobParameters _syncJobParameters;

		public SumSyncMetricsSink(ISyncServiceManager servicesManager, ISyncLog logger, IWorkspaceGuidService workspaceGuidService, SyncJobParameters syncJobParameters)
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
					LogSumMetricAsync(metricsManager, metric).GetAwaiter().GetResult();
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Logging to SUM failed. The metric with bucket '{bucket}' and workflow ID '{workflowId}' had value '{value}'.", metric.Name, metric.WorkflowId, metric.Value);
			}
		}

		private async Task LogSumMetricAsync(IMetricsManager metricsManager, Metric metric)
		{
			Guid workspaceGuid = await _workspaceGuidService.GetWorkspaceGuidAsync(_syncJobParameters.WorkspaceId).ConfigureAwait(false);

			MetricType metricType = metric.Type;
			switch (metricType)
			{
				case MetricType.PointInTimeString:
					await LogPointInTimeStringAsync(metricsManager, metric, workspaceGuid).ConfigureAwait(false);
					break;
				case MetricType.PointInTimeLong:
					await LogPointInTimeLongAsync(metricsManager, metric, workspaceGuid).ConfigureAwait(false);
					break;
				case MetricType.PointInTimeDouble:
					await LogPointInTimeDoubleAsync(metricsManager, metric, workspaceGuid).ConfigureAwait(false);
					break;
				case MetricType.TimedOperation:
					await LogTimedOperationAsync(metricsManager, metric, workspaceGuid).ConfigureAwait(false);
					break;
				case MetricType.Counter:
					await LogCounterOperationAsync(metricsManager, metric, workspaceGuid).ConfigureAwait(false);
					break;
				case MetricType.GaugeOperation:
					await LogGaugeOperationAsync(metricsManager, metric, workspaceGuid).ConfigureAwait(false);
					break;
				default:
					_logger.LogDebug("Logging metric type '{type}' to SUM is not implemented.", metricType);
					break;
			}
		}

		private static Task LogPointInTimeStringAsync(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			return metricsManager.LogPointInTimeStringAsync(metric.Name, workspaceGuid, metric.WorkflowId, metric.Value.ToString());
		}

		private static Task LogPointInTimeLongAsync(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			return metricsManager.LogPointInTimeLongAsync(metric.Name, workspaceGuid, metric.WorkflowId, (long)metric.Value);
		}

		private static Task LogPointInTimeDoubleAsync(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			return metricsManager.LogPointInTimeDoubleAsync(metric.Name, workspaceGuid, metric.WorkflowId, (double)metric.Value);
		}

		private static Task LogGaugeOperationAsync(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			return metricsManager.LogGaugeAsync(metric.Name, workspaceGuid, metric.WorkflowId, (long)metric.Value);
		}

		private static Task LogCounterOperationAsync(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			return metricsManager.LogCountAsync(metric.Name, workspaceGuid, metric.WorkflowId, 1);
		}

		private static Task LogTimedOperationAsync(IMetricsManager metricsManager, Metric metric, Guid workspaceGuid)
		{
			return metricsManager.LogTimerAsDoubleAsync(metric.Name, workspaceGuid, metric.WorkflowId, (double)metric.Value);
		}
	}
}