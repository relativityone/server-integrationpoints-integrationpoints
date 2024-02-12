using System;
using System.Collections.Generic;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry.RelEye;

namespace Relativity.Sync.Telemetry
{
    /// <summary>
    ///     Entry point for logging metrics. Dispatches metrics to registered <see cref="ISyncMetricsSink" />s for processing.
    /// </summary>
    internal class SyncMetrics : ISyncMetrics
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IEnumerable<ISyncMetricsSink> _sinks;
        private readonly IMetricsConfiguration _metricsConfiguration;
        private readonly IAPILog _log;

        /// <summary>
        ///     Creates a new instance of <see cref="SyncMetrics" /> with the given sinks.
        /// </summary>
        /// <param name="eventPublisher"></param>
        /// <param name="sinks">Sinks to which metrics should be sent</param>
        /// <param name="metricsConfiguration">Metrics configuration.</param>
        /// <param name="log"></param>
        public SyncMetrics(
            IEventPublisher eventPublisher,
            IEnumerable<ISyncMetricsSink> sinks,
            IMetricsConfiguration metricsConfiguration,
            IAPILog log)
        {
            _sinks = sinks;
            _metricsConfiguration = metricsConfiguration;
            _log = log;
            _eventPublisher = eventPublisher;
        }

        /// <inheritdoc />
        public void Send(IMetric metric)
        {
            metric.CorrelationId = _metricsConfiguration.CorrelationId;
            metric.ExecutingApplication = _metricsConfiguration.ExecutingApplication;
            metric.ExecutingApplicationVersion = _metricsConfiguration.ExecutingApplicationVersion;
            metric.DataSourceType = _metricsConfiguration.DataSourceType.GetDescription();
            metric.DataDestinationType = _metricsConfiguration.DataDestinationType.GetDescription();
            metric.IsRetry = _metricsConfiguration.JobHistoryToRetryId.HasValue;
            metric.SyncVersion = _metricsConfiguration.SyncVersion;

            if (_metricsConfiguration.RdoArtifactTypeId != (int)ArtifactType.Document &&
                _metricsConfiguration.DestinationRdoArtifactTypeId != (int)ArtifactType.Document)
            {
                metric.FlowName = TelemetryConstants.MetricIdentifiers.APM_FLOW_NAME_NON_DOCUMENT_OBJECTS;
            }
            else
            {
                metric.FlowName = _metricsConfiguration.ImageImport ? TelemetryConstants.MetricIdentifiers.APM_FLOW_NAME_IMAGES : TelemetryConstants.MetricIdentifiers.APM_FLOW_NAME_NATIVES_OR_METADATA;
            }

            try
            {
                foreach (ISyncMetricsSink sink in _sinks)
                {
                    sink.Send(metric);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to send Metric: {@Metric}.", metric);
            }
        }

        public void Send(IEvent @event)
        {
            try
            {
                _eventPublisher.Publish(@event);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to publish telemetry event: {@event}", @event);
            }
        }
    }
}
