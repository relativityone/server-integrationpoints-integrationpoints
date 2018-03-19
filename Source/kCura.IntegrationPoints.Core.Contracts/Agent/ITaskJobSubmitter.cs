using System;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.IntegrationPoints.Core.Contracts.Agent
{
	public interface ITaskJobSubmitter
	{
		void SubmitJob(object jobDetailsObject);
	}

	public class TaskJobSubmitter : ITaskJobSubmitter
	{
		private readonly IJobManager _jobManager;
		private readonly Job _parentJob;
		private readonly TaskType _taskToSubmit;
		private readonly Guid _batchInstance;
		public TaskJobSubmitter(IJobManager jobManager, Job parentJob, TaskType taskToSubmit, Guid batchInstance)
		{
			_jobManager = jobManager;
			_parentJob = parentJob;
			_taskToSubmit = taskToSubmit;
			_batchInstance = batchInstance;
		}

		public void SubmitJob(object jobDetailsObject)
		{
			TaskParameters taskParameters = new TaskParameters()
			{
				BatchInstance = _batchInstance,
				BatchParameters = jobDetailsObject
			};
			_jobManager.CreateJobWithTracker(_parentJob, taskParameters, _taskToSubmit, _batchInstance.ToString());
		}
	}
}
