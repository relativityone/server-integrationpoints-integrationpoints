using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using Job = kCura.IntegrationPoints.Data.Job;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public interface ITaskJobSubmitter
    {
        void SubmitJob(object jobDetailsObject);
    }

    public class TaskJobSubmitter : ITaskJobSubmitter
    {
        private readonly IJobManager _jobManager;
        private readonly IJobService _jobService;
        private readonly Job _parentJob;
        private readonly TaskType _taskToSubmit;

        public TaskJobSubmitter(IJobManager jobManager, IJobService jobService, Job parentJob, TaskType taskToSubmit)
        {
            _jobManager = jobManager;
            _jobService = jobService;
            _parentJob = parentJob;
            _taskToSubmit = taskToSubmit;
        }

        public void SubmitJob(object jobDetailsObject)
        {
            TaskParameters taskParameters = new TaskParameters()
            {
                BatchInstance = Guid.Parse(_parentJob.CorrelationID),
                BatchParameters = jobDetailsObject
            };
            Job job = _jobManager.CreateJobWithTracker(_parentJob, taskParameters, _taskToSubmit);

            _jobService.UpdateStopState(new List<long> { job.JobId }, StopState.None);
        }
    }
}
