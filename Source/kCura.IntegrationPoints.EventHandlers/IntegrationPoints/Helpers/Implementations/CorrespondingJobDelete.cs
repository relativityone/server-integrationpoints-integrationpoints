using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Contracts.Helpers;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Interfaces;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
    public class CorrespondingJobDelete : ICorrespondingJobDelete
    {
        private readonly IJobService _jobService;

        public CorrespondingJobDelete(IJobService jobService)
        {
            _jobService = jobService;
        }

        public void DeleteCorrespondingJob(int workspaceId, int integrationPointArtifactId)
        {
            List<string> taskTypes = TaskTypeHelper
                .GetManagerTypes()
                .Select(taskType => taskType.ToString())
                .ToList();

            IEnumerable<Job> jobs = _jobService.GetScheduledJobs(
                workspaceId,
                integrationPointArtifactId,
                taskTypes);

            foreach (Job job in jobs)
            {
                _jobService.DeleteJob(job.JobId);
            }
        }
    }
}