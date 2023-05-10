using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public class TaskExceptionService : ITaskExceptionService
    {
        private readonly IAPILog _logger;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobService _jobService;
        private readonly ISerializer _serializer;

        public TaskExceptionService(IAPILog logger, IJobHistoryErrorService jobHistoryErrorService, IJobHistoryService jobHistoryService, IJobService jobService, ISerializer serializer)
        {
            _logger = logger?.ForContext<TaskExceptionService>();
            _jobHistoryErrorService = jobHistoryErrorService;
            _jobHistoryService = jobHistoryService;
            _jobService = jobService;
            _serializer = serializer;
        }

        public void EndTaskWithError(ITask task, Exception ex)
        {
            var taskWithJobHistory = task as ITaskWithJobHistory;
            if (taskWithJobHistory == null)
            {
                return;
            }

            try
            {
                _jobHistoryErrorService.JobHistory = taskWithJobHistory.JobHistory;
                _jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, ex.Message, ex.StackTrace);
                taskWithJobHistory.JobHistory.JobStatus = JobStatusChoices.JobHistoryErrorJobFailed;
                _jobHistoryService.UpdateRdoWithoutDocuments(taskWithJobHistory.JobHistory);
                _jobService.CleanupJobQueueTable();
            }
            catch (Exception errorHandlingException)
            {
                _logger.LogError(errorHandlingException, "An error occured ending task with error.");
                throw;
            }
        }

        public void EndJobWithError(Job job, Exception ex)
        {
            // error occured while job initialization
            if (job == null)
            {
                return;
            }

            try
            {
                TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
                JobHistory jobHistory = _jobHistoryService.GetRdoWithoutDocuments(taskParameters.BatchInstance);
                SetJobIdIfNotPresent(jobHistory, job);

                _jobHistoryErrorService.JobHistory = jobHistory;
                _jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);

                jobHistory.JobStatus = JobStatusChoices.JobHistoryErrorJobFailed;
                _jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);
            }
            catch (Exception errorHandlingException)
            {
                _logger.LogError(errorHandlingException, "An error occured ending job with error.");
                throw;
            }
        }

        private void SetJobIdIfNotPresent(JobHistory jobHistory, Job job)
        {
            if (jobHistory == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(jobHistory.JobID))
            {
                long jobId = job.RootJobId ?? job.JobId;
                jobHistory.JobID = jobId.ToString();
            }
        }
    }
}
