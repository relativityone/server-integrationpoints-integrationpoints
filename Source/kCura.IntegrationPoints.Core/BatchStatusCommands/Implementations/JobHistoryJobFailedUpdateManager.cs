using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
    public class JobHistoryJobFailedUpdateManager : IBatchStatus
    {
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IAPILog _log;

        public JobHistoryJobFailedUpdateManager(IJobHistoryErrorService jobHistoryErrorService, IAPILog log)
        {
            _jobHistoryErrorService = jobHistoryErrorService;
            _log = log;
        }

        public void OnJobComplete(Job job)
        {
        }

        public void OnJobStart(Job job)
        {
            if(job.JobFailed != null)
            {
                _log.LogError(job.JobFailed.Exception, "Marking Job {jobId} as failed with following exception", job.JobId);
                _jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, job.JobFailed.Exception);
                _jobHistoryErrorService.CommitErrors();
            }
        }
	}
}
