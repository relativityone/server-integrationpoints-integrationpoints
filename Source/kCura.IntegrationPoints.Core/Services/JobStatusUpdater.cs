using System;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client.DTOs;
using Relativity.Telemetry.APM;
using Client = Relativity.Telemetry.APM.Client;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobStatusUpdater : IJobStatusUpdater
	{
		private readonly IJobHistoryService _jobHistoryService;
		private readonly JobHistoryErrorQuery _service;

		public JobStatusUpdater(JobHistoryErrorQuery service, IJobHistoryService jobHistoryService)
		{
			_service = service;
			_jobHistoryService = jobHistoryService;
		}

		public Choice GenerateStatus(Guid batchId)
		{
			Data.JobHistory result = _jobHistoryService.GetRdo(batchId);
			return GenerateStatus(result);
		}

		public Choice GenerateStatus(Data.JobHistory jobHistory)
		{
			if (jobHistory == null)
			{
				throw new ArgumentNullException(nameof(jobHistory));
			}

			if (jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopping))
			{
				return JobStatusChoices.JobHistoryStopped;
			}

			JobHistoryError recent = _service.GetJobErrorFailedStatus(jobHistory.ArtifactId);
			if (recent != null)
			{
				if (recent.ErrorType.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorItem))
				{
					return JobStatusChoices.JobHistoryCompletedWithErrors;
				}

				if (recent.ErrorType.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob))
				{
					return jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryValidationFailed)
						? JobStatusChoices.JobHistoryValidationFailed
						: JobStatusChoices.JobHistoryErrorJobFailed;
				}
			}
			else
			{
				if (jobHistory.ItemsWithErrors.GetValueOrDefault(0) > 0)
				{
					return JobStatusChoices.JobHistoryCompletedWithErrors;
				}
			}

			return JobStatusChoices.JobHistoryCompleted;
		}

		public void SendHealthCheck(Data.JobHistory jobHistory, long workspaceID)
		{
			if (IsJobFailed(jobHistory.JobStatus))
			{
				IHealthMeasure healthcheck = Client.APMClient.HealthCheckOperation(Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK, () => HealthCheck.CreateJobFailedMetric(jobHistory, workspaceID));
				healthcheck.Write();
			}
		}

		private static bool IsJobFailed(Choice jobStatusChoice)
		{
			return jobStatusChoice.EqualsToChoice(JobStatusChoices.JobHistoryValidationFailed) || jobStatusChoice.EqualsToChoice(JobStatusChoices.JobHistoryErrorJobFailed);
		}
	}
}