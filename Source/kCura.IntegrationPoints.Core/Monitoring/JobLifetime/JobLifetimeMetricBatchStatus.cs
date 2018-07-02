using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Monitoring.JobLifetimeMessages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecordsMessages;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Relativity.DataTransfer.MessageService;

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

		public JobLifetimeMetricBatchStatus(IMessageService messageService, IIntegrationPointService integrationPointService,
			IProviderTypeService providerTypeService, IJobStatusUpdater updater, IJobHistoryService jobHistoryService, ISerializer serializer, 
			IDateTimeHelper dateTimeHelper)
		{
			_messageService = messageService;
			_integrationPointService = integrationPointService;
			_providerTypeService = providerTypeService;
			_updater = updater;
			_jobHistoryService = jobHistoryService;
			_serializer = serializer;
			_dateTimeHelper = dateTimeHelper;
		}

		public void OnJobStart(Job job)
		{

		}

		public void OnJobComplete(Job job)
		{
			ProviderType providerType = GetProviderType(job);
			JobHistory jobHistory = GetHistory(job);
			Choice status = _updater.GenerateStatus(jobHistory, job.JobId);

			SendLifetimeMessage(status, providerType);
			SendRecordsMessage(providerType, jobHistory);
			SendThroughputMessage(providerType, jobHistory);
		}

		private void SendRecordsMessage(ProviderType providerType, JobHistory jobHistory)
		{
			long? totalRecords = jobHistory.TotalItems;
			int? completedRecords = jobHistory.ItemsTransferred;
			_messageService.Send(new JobTotalRecordsCountMessage { Provider = providerType.ToString(), TotalRecordsCount = totalRecords ?? 0 });
			_messageService.Send(new JobCompletedRecordsCountMessage { Provider = providerType.ToString(), CompletedRecordsCount = completedRecords ?? 0 });
		}

		private void SendLifetimeMessage(Choice status, ProviderType providerType)
		{
			if (status.EqualsToChoice(JobStatusChoices.JobHistoryErrorJobFailed))
			{
				_messageService.Send(new JobFailedMessage { Provider = providerType.ToString() });
			}
			else if (status.EqualsToChoice(JobStatusChoices.JobHistoryValidationFailed))
			{
				_messageService.Send(new JobValidationFailedMessage { Provider = providerType.ToString() });
			}
			else if (
				status.EqualsToChoice(JobStatusChoices.JobHistoryCompleted) ||
				status.EqualsToChoice(JobStatusChoices.JobHistoryCompletedWithErrors) ||
				status.EqualsToChoice(JobStatusChoices.JobHistoryStopped))
			{
				_messageService.Send(new JobCompletedMessage { Provider = providerType.ToString() });
			}
		}

		private void SendThroughputMessage(ProviderType providerType, JobHistory jobHistory)
		{
			int? completedRecords = jobHistory.ItemsTransferred;
			TimeSpan? duration = (jobHistory.EndTimeUTC ?? _dateTimeHelper.Now()) - jobHistory.StartTimeUTC;
		
			if (completedRecords > 0 && duration.HasValue)
			{
				double throughput = completedRecords.Value / duration.Value.TotalSeconds;
				_messageService.Send(new JobThroughputMessage {Provider = providerType.ToString(), RecordsPerSecond = throughput});
			}
		}

		private JobHistory GetHistory(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			return _jobHistoryService.GetRdo(taskParameters.BatchInstance);
		}

		private ProviderType GetProviderType(Job job)
		{
			IntegrationPoint integrationPoint = _integrationPointService.GetRdo(job.RelatedObjectArtifactID);
			ProviderType providerType = integrationPoint.GetProviderType(_providerTypeService);
			return providerType;
		}
	}
}
