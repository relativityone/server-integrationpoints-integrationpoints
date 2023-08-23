using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Monitoring.JobLifetime
{
    public class JobLifetimeMetricBatchStatus : IBatchStatus
    {
        private readonly IMessageService _messageService;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IProviderTypeService _providerTypeService;
        private readonly IJobStatusUpdater _updater;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly ISerializer _serializer;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAPILog _log;

        public JobLifetimeMetricBatchStatus(
            IMessageService messageService,
            IIntegrationPointService integrationPointService,
            IProviderTypeService providerTypeService,
            IJobStatusUpdater updater,
            IJobHistoryService jobHistoryService,
            ISerializer serializer,
            IDateTimeHelper dateTimeHelper,
            IAPILog log)
        {
            _messageService = messageService;
            _integrationPointService = integrationPointService;
            _providerTypeService = providerTypeService;
            _updater = updater;
            _jobHistoryService = jobHistoryService;
            _serializer = serializer;
            _dateTimeHelper = dateTimeHelper;
            _log = log;
        }

        public void OnJobStart(Job job)
        {
        }

        public void OnJobComplete(Job job)
        {
            string providerName = GetProviderName(job);
            JobHistory jobHistory = GetHistory(job);
            ChoiceRef status = _updater.GenerateStatus(jobHistory, job.JobId);
            string correlationId = job.CorrelationID;

            _log.LogInformation("On Lifetime Metric - BatchInstance {batchInstanceId}, Status {status}",
                correlationId, status?.Name);
            if (IsJobEnd(status))
            {
                _log.LogInformation("Sending Statistics Metrics when Job End");
                SendRecordsMessage(providerName, jobHistory, correlationId);
                SendThroughputMessage(providerName, jobHistory, correlationId);
            }

            SendLifetimeMessage(status, providerName, correlationId);
        }

        private bool IsJobEnd(ChoiceRef status)
        {
            return !status.EqualsToChoice(JobStatusChoices.JobHistorySuspended);
        }

        private void SendRecordsMessage(string providerName, JobHistory jobHistory, string correlationId)
        {
            long? totalRecords = jobHistory.TotalItems;
            int? completedRecords = jobHistory.ItemsTransferred;
            _messageService.Send(new JobTotalRecordsCountMessage
            {
                Provider = providerName,
                CorrelationID = correlationId,
                TotalRecordsCount = totalRecords ?? 0
            });
            _messageService.Send(new JobCompletedRecordsCountMessage
            {
                Provider = providerName,
                CorrelationID = correlationId,
                CompletedRecordsCount = completedRecords ?? 0
            });
        }

        private void SendLifetimeMessage(ChoiceRef status, string providerName, string correlationId)
        {
            if (status.EqualsToChoice(JobStatusChoices.JobHistoryErrorJobFailed))
            {
                _messageService.Send(new JobFailedMessage
                {
                    Provider = providerName,
                    CorrelationID = correlationId
                });
            }
            else if (status.EqualsToChoice(JobStatusChoices.JobHistoryValidationFailed))
            {
                _messageService.Send(new JobValidationFailedMessage
                {
                    Provider = providerName,
                    CorrelationID = correlationId
                });
            }
            else if (
                status.EqualsToChoice(JobStatusChoices.JobHistoryCompleted) ||
                status.EqualsToChoice(JobStatusChoices.JobHistoryCompletedWithErrors) ||
                status.EqualsToChoice(JobStatusChoices.JobHistoryStopped))
            {
                _messageService.Send(new JobCompletedMessage
                {
                    Provider = providerName,
                    CorrelationID = correlationId
                });
            }
            else if (status.EqualsToChoice(JobStatusChoices.JobHistorySuspended))
            {
                _messageService.Send(new JobSuspendedMessage
                {
                    Provider = providerName,
                    CorrelationID = correlationId
                });
            }
        }

        private void SendThroughputMessage(string providerName, JobHistory jobHistory, string correlationId)
        {
            int? completedRecords = jobHistory.ItemsTransferred;
            TimeSpan? duration = (jobHistory.EndTimeUTC ?? _dateTimeHelper.Now()) - jobHistory.StartTimeUTC;

            if (completedRecords > 0 && duration.HasValue)
            {
                double throughput = completedRecords.Value / duration.Value.TotalSeconds;
                _messageService.Send(new JobThroughputMessage
                {
                    Provider = providerName,
                    CorrelationID = correlationId,
                    RecordsPerSecond = throughput
                });
            }
        }

        private JobHistory GetHistory(Job job)
        {
            return _jobHistoryService.GetRdoWithoutDocuments(Guid.Parse(job.CorrelationID));
        }

        private string GetProviderName(Job job)
        {
            IntegrationPointSlimDto integrationPoint = _integrationPointService.ReadSlim(job.RelatedObjectArtifactID);

            return integrationPoint.GetProviderName(_providerTypeService);
        }
    }
}
