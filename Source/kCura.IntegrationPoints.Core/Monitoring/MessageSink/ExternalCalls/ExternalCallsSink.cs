using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.Tools;
using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation.Model;

namespace kCura.IntegrationPoints.Core.Monitoring.MessageSink.ExternalCalls
{
    internal class ExternalCallsSink :
        IMessageSink<ExternalCallCompletedMessage>,
        IMessageSink<JobStartedMessage>,
        IMessageSink<JobCompletedMessage>,
        IMessageSink<JobFailedMessage>
    {
        private IDictionary<string, ExternalCallsJobSummaryMessage> _statisticsAggregatedByServiceType;
        private string _currentCorrelationId;
        private const string _BUCKET_EXTERNALL_CALL = "IntegrationPoints.Performance.ExternalCall";
        private const string _BUCKET_EXTERNALL_CALL_JOB_SUMMARY = "IntegrationPoints.Performance.ExternalCall.Summary";
        private readonly IAPILog _logger;
        private readonly object _lock = new object();
        private readonly Lazy<IMetricsManager> _apmManager;

        public ExternalCallsSink(IMetricsManagerFactory metricsManagerFactory, IAPILog logger)
        {
            _apmManager = new Lazy<IMetricsManager>(metricsManagerFactory.CreateAPMManager);
            _logger = logger;
        }

        public void OnMessage(ExternalCallCompletedMessage message)
        {
            if (message == null)
            {
                return;
            }
            ValidateAndUpdateAggregatedStatistics(message);
            _apmManager.Value.LogCount(_BUCKET_EXTERNALL_CALL, message.Duration, message);
        }

        public void OnMessage(JobStartedMessage message)
        {
            if (message != null)
            {
                StartJob(message);
            }
        }

        public void OnMessage(JobCompletedMessage message)
        {
            if (message != null)
            {
                CompleteJob(message.CorrelationID);
            }
        }

        public void OnMessage(JobFailedMessage message)
        {
            if (message != null)
            {
                CompleteJob(message.CorrelationID);
            }
        }

        private void ValidateAndUpdateAggregatedStatistics(ExternalCallCompletedMessage message)
        {
            bool isCorrelationIdInvalid = false;
            string currentCorrelationId = null;
            lock (_lock)
            {
                if (_currentCorrelationId == message.CorrelationID)
                {
                    UpdateAggregatedStatistics(message);
                }
                else
                {
                    isCorrelationIdInvalid = true;
                    currentCorrelationId = _currentCorrelationId;
                }
            }

            if (isCorrelationIdInvalid)
            {
                _logger.LogWarning("Received external call metric with invalid CorrelationId. Current correlationId: {current}, send: {send}", currentCorrelationId, message.CorrelationID);
            }
        }

        private void UpdateAggregatedStatistics(ExternalCallCompletedMessage message)
        {
            if (!_statisticsAggregatedByServiceType.ContainsKey(message.ServiceType))
            {
                _statisticsAggregatedByServiceType[message.ServiceType] = new ExternalCallsJobSummaryMessage(message, message.ServiceType);
            }

            _statisticsAggregatedByServiceType[message.ServiceType].TotalCount++;
            _statisticsAggregatedByServiceType[message.ServiceType].TotalDuration += message.Duration;
            if (message.HasFailed)
            {
                _statisticsAggregatedByServiceType[message.ServiceType].FailedCount++;
                _statisticsAggregatedByServiceType[message.ServiceType].FailedDuration += message.Duration;
            }
        }

        private void StartJob(JobStartedMessage message)
        {
            Tuple<string, IDictionary<string, ExternalCallsJobSummaryMessage>> changeResult = ChangeCorrelationId(message.CorrelationID);
            string previousCorrelationId = changeResult.Item1;
            IDictionary<string, ExternalCallsJobSummaryMessage> previousDictionary = changeResult.Item2;

            if (previousCorrelationId != null)
            {
                _logger.LogWarning("Job started before previous was completed, job context will be updated. Previous correlationId {previous}, current: {current}",
                    previousCorrelationId, message.CorrelationID);

                SendSummary(previousDictionary);
            }
        }

        private void CompleteJob(string correlationId)
        {
            Tuple<string, IDictionary<string, ExternalCallsJobSummaryMessage>> changeResult = ChangeCorrelationId();
            string previousCorrelationId = changeResult.Item1;
            IDictionary<string, ExternalCallsJobSummaryMessage> previousDictionary = changeResult.Item2;

            SendSummary(previousDictionary);
            if (previousCorrelationId != correlationId)
            {
                _logger.LogWarning("Job completed correlationId does not match context correlationId. Context: {previous}, message: {current}",
                    previousCorrelationId, correlationId);
            }
        }

        private Tuple<string, IDictionary<string, ExternalCallsJobSummaryMessage>> ChangeCorrelationId(string newCorrelationId = null)
        {
            string previousCorrelationId;
            IDictionary<string, ExternalCallsJobSummaryMessage> previousDictionary;
            lock (_lock)
            {
                previousCorrelationId = _currentCorrelationId;
                previousDictionary = _statisticsAggregatedByServiceType;
                _currentCorrelationId = newCorrelationId;
                _statisticsAggregatedByServiceType = newCorrelationId == null ? null : new Dictionary<string, ExternalCallsJobSummaryMessage>();
            }
            return new Tuple<string, IDictionary<string, ExternalCallsJobSummaryMessage>>(previousCorrelationId, previousDictionary);
        }

        private void SendSummary(IDictionary<string, ExternalCallsJobSummaryMessage> aggregatedStatistics)
        {
            if (aggregatedStatistics == null)
            {
                return;
            }

            foreach (ExternalCallsJobSummaryMessage externalCallsJobSummaryMessage in aggregatedStatistics.Values)
            {
                _apmManager.Value.LogCount(_BUCKET_EXTERNALL_CALL_JOB_SUMMARY, externalCallsJobSummaryMessage.TotalDuration, externalCallsJobSummaryMessage);
            }
        }
    }
}
