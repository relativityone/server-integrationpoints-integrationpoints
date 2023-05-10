using System;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Services
{
    public class JobStatusUpdater : IJobStatusUpdater
    {
        private readonly IJobHistoryService _jobHistoryService;
        private readonly JobHistoryErrorQuery _service;
        private readonly IJobService _jobService;

        public JobStatusUpdater(JobHistoryErrorQuery service, IJobHistoryService jobHistoryService, IJobService jobService)
        {
            _service = service;
            _jobHistoryService = jobHistoryService;
            _jobService = jobService;
        }

        public ChoiceRef GenerateStatus(Guid batchId)
        {
            Data.JobHistory result = _jobHistoryService.GetRdoWithoutDocuments(batchId);
            return GenerateStatus(result);
        }

        public ChoiceRef GenerateStatus(Data.JobHistory jobHistory, long? jobId = null)
        {
            if (jobHistory == null)
            {
                throw new ArgumentNullException(nameof(jobHistory));
            }

            if (jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopping))
            {
                return JobStatusChoices.JobHistoryStopped;
            }

            if (jobId.HasValue)
            {
                Job batchJob = _jobService.GetJob(jobId.Value);

                if (batchJob == null)
                {
                    throw new InvalidOperationException($"Cannot find job with ID: {jobId}");
                }

                if (batchJob.StopState.HasFlag(StopState.DrainStopping) || batchJob.StopState.HasFlag(StopState.DrainStopped))
                {
                    return JobStatusChoices.JobHistorySuspended;
                }
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
    }
}
