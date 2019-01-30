﻿using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Core.Models;
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

		public JobLifetimeMetricBatchStatus(
			IMessageService messageService, 
			IIntegrationPointService integrationPointService,
			IProviderTypeService providerTypeService, 
			IJobStatusUpdater updater, 
			IJobHistoryService jobHistoryService, 
			ISerializer serializer, 
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
			Choice status = _updater.GenerateStatus(jobHistory);
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			string correlationId = taskParameters.BatchInstance.ToString();

			SendRecordsMessage(providerType, jobHistory, correlationId);
			SendThroughputMessage(providerType, jobHistory, correlationId);
			SendLifetimeMessage(status, providerType, correlationId);
		}

		private void SendRecordsMessage(ProviderType providerType, JobHistory jobHistory, string correlationId)
		{
			long? totalRecords = jobHistory.TotalItems;
			int? completedRecords = jobHistory.ItemsTransferred;
			_messageService.Send(new JobTotalRecordsCountMessage
			{
				Provider = providerType.ToString(),
				CorrelationID = correlationId,
				TotalRecordsCount = totalRecords ?? 0
			});
			_messageService.Send(new JobCompletedRecordsCountMessage
			{
				Provider = providerType.ToString(),
				CorrelationID = correlationId,
				CompletedRecordsCount = completedRecords ?? 0
			});
		}

		private void SendLifetimeMessage(Choice status, ProviderType providerType, string correlationId)
		{
			if (status.EqualsToChoice(JobStatusChoices.JobHistoryErrorJobFailed))
			{
				_messageService.Send(new JobFailedMessage
				{
					Provider = providerType.ToString(),
					CorrelationID = correlationId
				});
			}
			else if (status.EqualsToChoice(JobStatusChoices.JobHistoryValidationFailed))
			{
				_messageService.Send(new JobValidationFailedMessage
				{
					Provider = providerType.ToString(),
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
					Provider = providerType.ToString(),
					CorrelationID = correlationId
				});
			}
		}

		private void SendThroughputMessage(ProviderType providerType, JobHistory jobHistory, string correlationId)
		{
			int? completedRecords = jobHistory.ItemsTransferred;
			TimeSpan? duration = (jobHistory.EndTimeUTC ?? _dateTimeHelper.Now()) - jobHistory.StartTimeUTC;
		
			if (completedRecords > 0 && duration.HasValue)
			{
				double throughput = completedRecords.Value / duration.Value.TotalSeconds;
				_messageService.Send(new JobThroughputMessage
				{
					Provider = providerType.ToString(),
					CorrelationID = correlationId,
					RecordsPerSecond = throughput
				});
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
