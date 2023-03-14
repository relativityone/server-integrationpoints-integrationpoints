using System;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public static class AgentExceptionHelper
    {
        public static void HandleException(IJobHistoryErrorService jobHistoryErrorService, IJobHistoryService jobHistoryService, IAPILog apiLog,
            Exception ex, Job job, TaskResult result, JobHistory jobHistory)
        {
            apiLog.LogError(ex, "Failed to execute tasks in job {JobId}.", job.JobId);

            result.Status = TaskStatusEnum.Fail;
            jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
            if (ex is PermissionException || ex is IntegrationPointValidationException)
            {
                jobHistory.JobStatus = JobStatusChoices.JobHistoryValidationFailed;
                jobHistoryService.UpdateRdoToBeChanged(jobHistory);
            }
        }
    }
}
