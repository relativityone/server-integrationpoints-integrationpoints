using System;
using kCura.IntegrationPoints.Core.Monitoring;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Queries;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobStatusUpdater : IJobStatusUpdater
	{
		private readonly JobHistoryErrorQuery _service;
		private readonly IJobHistoryService _jobHistoryService;

		public JobStatusUpdater(JobHistoryErrorQuery service, IJobHistoryService jobHistoryService)
		{
			_service = service;
			_jobHistoryService = jobHistoryService;
		}

		public Relativity.Client.DTOs.Choice GenerateStatus(Guid batchId, long wkspId)
		{
			Data.JobHistory result = _jobHistoryService.GetRdo(batchId);
			return GenerateStatus(result, wkspId);
		}

		public Relativity.Client.DTOs.Choice GenerateStatus(Data.JobHistory jobHistory, long wkspId)
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
				if (recent.ErrorType.EqualsToChoice(Data.ErrorTypeChoices.JobHistoryErrorItem))
				{
					return Data.JobStatusChoices.JobHistoryCompletedWithErrors;
				}
				if (recent.ErrorType.EqualsToChoice(Data.ErrorTypeChoices.JobHistoryErrorJob))
				{
					IHealthMeasure healthcheck = Client.APMClient.HealthCheckOperation(Constants.IntegrationPoints.Telemetry.APM_HEALTHCHECK, 
						() => HealthCheck.CreateJobFailedMetric(jobHistory, wkspId));
					healthcheck.Write();

					return Data.JobStatusChoices.JobHistoryErrorJobFailed;
				}
			}
			else
			{
				if (jobHistory.ItemsWithErrors.GetValueOrDefault(0) > 0)
				{
					return Data.JobStatusChoices.JobHistoryCompletedWithErrors;
				}
			}
			return Data.JobStatusChoices.JobHistoryCompleted;
		}
	}
}