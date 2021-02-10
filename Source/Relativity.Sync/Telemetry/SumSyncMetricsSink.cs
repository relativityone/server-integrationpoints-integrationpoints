using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		public void Send(IMetric metric)
		{
			SendAsync(metric).GetAwaiter().GetResult();
		}

		private async Task SendAsync(IMetric metric)
		{
			using (IMetricsManager metricsManager = _servicesManager.CreateProxy<IMetricsManager>(ExecutionIdentity.System))
			{
				foreach (var sumMetric in await GetSumMetricsAsync(metric).ConfigureAwait(false))
				{
					try
					{
						await SendSumMetricAsync(metricsManager, sumMetric).ConfigureAwait(false);
					}
					catch (Exception e)
					{
						_logger.LogError(e, "Logging to SUM failed. The metric with bucket '{bucket}' and workflow ID '{workflowId}' had value '{value}'.",
							sumMetric.Bucket, sumMetric.WorkflowId, sumMetric.Value);
					}
				}
			}
		}

		private async Task<IEnumerable<SumMetric>> GetSumMetricsAsync(IMetric metric)
		{
			Guid workspaceGuid = await _workspaceGuidService.GetWorkspaceGuidAsync(_syncJobParameters.WorkspaceId).ConfigureAwait(false);

			var sumMetrics = ReadSumMetrics(metric);

			sumMetrics.ForEach(x => x.WorkspaceGuid = workspaceGuid);

			return sumMetrics;
		}

		private IList<SumMetric> ReadSumMetrics(IMetric metric)
		{
			return metric.GetMetricProperties()
				.Where(p => p.GetCustomAttribute<MetricAttribute>() != null && p.GetValue(this) != null)
				.Select(p =>
				{
					var attr = p.GetCustomAttribute<MetricAttribute>();

					return new SumMetric
					{
						Type = attr.Type,
						Bucket = attr.Name ?? metric.Name,
						Value = p.GetValue(metric),
						WorkflowId = metric.WorkflowId
					};
				}).ToList();
		}

		private Task SendSumMetricAsync(IMetricsManager metricsManager, SumMetric metric)
		{
			switch (metric.Type)
			{
				case MetricType.PointInTimeString:
					return metricsManager.LogPointInTimeStringAsync(metric.Bucket, metric.WorkspaceGuid, metric.WorkflowId, metric.Value.ToString());
				case MetricType.PointInTimeLong:
					return metricsManager.LogPointInTimeLongAsync(metric.Bucket, metric.WorkspaceGuid, metric.WorkflowId, (long)metric.Value);
				case MetricType.PointInTimeDouble:
					return metricsManager.LogPointInTimeDoubleAsync(metric.Bucket, metric.WorkspaceGuid, metric.WorkflowId, (double)metric.Value);
				case MetricType.TimedOperation:
					return metricsManager.LogTimerAsDoubleAsync(metric.Bucket, metric.WorkspaceGuid, metric.WorkflowId, (double)metric.Value);
				case MetricType.Counter:
					return metricsManager.LogCountAsync(metric.Bucket, metric.WorkspaceGuid, metric.WorkflowId, 1);
				case MetricType.GaugeOperation:
					return metricsManager.LogGaugeAsync(metric.Bucket, metric.WorkspaceGuid, metric.WorkflowId, (long)metric.Value);
				default:
					_logger.LogDebug("Logging metric type '{type}' to SUM is not implemented.", metric.Type);
					return Task.CompletedTask;
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