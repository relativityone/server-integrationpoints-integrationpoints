using System;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Exceptions;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public class AgentExceptionHelper
    {
        public static void HandleException(
            IJobHistoryErrorService jobHistoryErrorService,
            IJobHistoryService jobHistoryService,
            ILogger<AgentExceptionHelper> logger,
            Exception ex,
            Job job,
            TaskResult result,
            JobHistory jobHistory)
        {
            logger.LogError(ex, "Failed to execute tasks in job {JobId}.", job.JobId);

            result.Status = TaskStatusEnum.Fail;
            jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
            if (ex is PermissionException || ex is IntegrationPointValidationException)
            {
                jobHistory.JobStatus = JobStatusChoices.JobHistoryValidationFailed;
                jobHistoryService.UpdateRdoWithoutDocuments(jobHistory);
            }
        }
    }
}
