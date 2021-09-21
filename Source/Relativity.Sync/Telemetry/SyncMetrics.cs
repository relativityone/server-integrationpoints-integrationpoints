﻿using System.Collections.Generic;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Telemetry
{
	/// <summary>
	///     Entry point for logging metrics. Dispatches metrics to registered <see cref="ISyncMetricsSink" />s for processing.
	/// </summary>
	internal class SyncMetrics : ISyncMetrics
	{
		private readonly IEnumerable<ISyncMetricsSink> _sinks;
		private readonly IMetricsConfiguration _metricsConfiguration;

		/// <summary>
		///     Creates a new instance of <see cref="SyncMetrics" /> with the given sinks.
		/// </summary>
		/// <param name="sinks">Sinks to which metrics should be sent</param>
		/// <param name="metricsConfiguration">Metrics configuration.</param>
		public SyncMetrics(IEnumerable<ISyncMetricsSink> sinks, IMetricsConfiguration metricsConfiguration)
		{
			_sinks = sinks;
			_metricsConfiguration = metricsConfiguration;
		}

		/// <inheritdoc />
		public void Send(IMetric metric)
		{
			metric.CorrelationId = _metricsConfiguration.CorrelationId;
			metric.ExecutingApplication = _metricsConfiguration.ExecutingApplication;
			metric.ExecutingApplicationVersion = _metricsConfiguration.ExecutingApplicationVersion;
			metric.DataSourceType = _metricsConfiguration.DataSourceType;
			metric.DataDestinationType = _metricsConfiguration.DataDestinationType;
			metric.IsRetry = _metricsConfiguration.JobHistoryToRetryId.HasValue;
			metric.FlowName = _metricsConfiguration.ImageImport ? TelemetryConstants.MetricIdentifiers.APM_FLOW_NAME_IMAGES : TelemetryConstants.MetricIdentifiers.APM_FLOW_NAME_NATIVES_OR_METADATA;
			metric.SyncVersion = _metricsConfiguration.SyncVersion;
			
			foreach (ISyncMetricsSink sink in _sinks)
			{
				sink.Send(metric);
			}
		}
	}
}