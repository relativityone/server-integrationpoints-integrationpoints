using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Contracts.Agent
{
	public interface ITaskJobSubmitter
	{
		void SubmitJob(string jobDetails);
	}

	public class TaskJobSubmitter : ITaskJobSubmitter
	{
		private readonly IJobManager _jobManager;
		private readonly Job _parentJob;
		private readonly TaskType _taskToSubmit;
		public TaskJobSubmitter(IJobManager jobManager, Job parentJob, TaskType taskToSubmit)
		{
			_jobManager = jobManager;
			_parentJob = parentJob;
			_taskToSubmit = taskToSubmit;
		}

		public void SubmitJob(string jobDetails)
		{
			_jobManager.CreateJob(jobDetails, _taskToSubmit, _parentJob.WorkspaceID, _parentJob.RelatedObjectArtifactID);
		}
	}
}
