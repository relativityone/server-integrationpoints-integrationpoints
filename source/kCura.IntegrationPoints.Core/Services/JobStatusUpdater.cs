using System;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Queries;

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

		public Relativity.Client.Choice GenerateStatus(Guid batchId)
		{
			var result = _jobHistoryService.GetRdo(batchId);
			return GenerateStatus(result);
		}

		public Relativity.Client.Choice GenerateStatus(Data.JobHistory jobHistory)
		{
			if (jobHistory == null)
			{
				throw new ArgumentNullException("job History");
			}
			var recent = _service.GetJobErrorFailedStatus(jobHistory.ArtifactId);
			if (recent != null)
			{
				if (recent.ErrorType.EqualsToChoice(Data.ErrorTypeChoices.JobHistoryErrorItem))
				{
					return Data.JobStatusChoices.JobHistoryCompletedWithErrors;
				}
				if (recent.ErrorType.EqualsToChoice(Data.ErrorTypeChoices.JobHistoryErrorJob))
				{
					return Data.JobStatusChoices.JobHistoryErrorJobFailed;
				}
			}
			else
			{
				if (jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopping))
				{
					return JobStatusChoices.JobHistoryStopped;
				}
				if (jobHistory.ItemsWithErrors.GetValueOrDefault(0) > 0)
				{
					return Data.JobStatusChoices.JobHistoryCompletedWithErrors;
				}
			}
			return Data.JobStatusChoices.JobHistoryCompleted;
		}
	}
}