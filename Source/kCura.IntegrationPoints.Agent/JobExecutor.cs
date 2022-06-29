using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Agent.Interfaces;
using kCura.IntegrationPoints.Agent.Logging;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent
{
	public delegate void ExceptionEventHandler(Job job, ITask task, Exception exception);

	public delegate TaskResult JobPostExecuteEventHandler(Job job);

	internal class JobExecutor : IJobExecutor
	{
		private const string PROCESSING_JOB_MESSAGE_TEMPLATE = "Processing Job ID : {0} : Workspace ID {1} : Job Type {2}";
		private const string START_PROCESSING_JOB_MESSAGE_TEMPLATE = "Started : " + PROCESSING_JOB_MESSAGE_TEMPLATE;
		private const string FINISHED_PROCESSING_JOB_MESSAGE_TEMPLATE = "Finished : " + PROCESSING_JOB_MESSAGE_TEMPLATE;

		private readonly ITaskProvider _taskProvider;
		private readonly IAgentNotifier _agentNotifier;
		private readonly IJobService _jobService;
		private IAPILog _logger { get; }

		public event ExceptionEventHandler JobExecutionError;

		public JobExecutor(ITaskProvider taskProvider, IAgentNotifier agentNotifier, IJobService jobService, IAPILog logger)
		{
			if (taskProvider == null)
			{
				throw new ArgumentNullException(nameof(taskProvider));
			}

			if (agentNotifier == null)
			{
				throw new ArgumentNullException(nameof(agentNotifier));
			}

			if (jobService == null)
			{
				throw new ArgumentNullException(nameof(jobService));
			}

			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			_taskProvider = taskProvider;
			_agentNotifier = agentNotifier;
			_jobService = jobService;
			_logger = logger;
		}

		public TaskResult ProcessJob(Job job)
		{
			if (job == null)
			{
				throw new ArgumentNullException(nameof(job));
			}

			InitializeJobExecution(job);
			return ExecuteJob(job);
		}

		private TaskResult GetTask(Job job, ref ITask task)
		{
			try
			{
				task = _taskProvider.GetTask(job);
				if (task == null)
				{
					throw new IntegrationPointsException($"Could not find corresponding Task. JobId: {job?.JobId}, IntegrationPoint: {job?.RelatedObjectArtifactID}");
				}
				return new TaskResult { Status = TaskStatusEnum.Success };
			}
			catch (Exception ex)
			{
				RaiseJobExecutionErrorEvent(job, task, ex);

				return new TaskResult
				{
					Status = TaskStatusEnum.Fail,
					Exceptions = new List<Exception> { ex }
				};
			}
		}

		private TaskResult ExecuteJob(Job job)
		{
			LogJobState(job, JobLogState.Started);
			LogStartingExecuteTask(job);

			ITask task = null;
			TaskResult resultOfGetTask = GetTask(job, ref task);
			if (resultOfGetTask.Status == TaskStatusEnum.Fail)
			{
				return resultOfGetTask;
			}

			return ExecuteTask(task, job);
		}

		private TaskResult ExecuteTask(ITask task, Job job)
		{
			try
			{
				task.Execute(job);

				LogJobState(job, JobLogState.Finished);
				string msg = string.Format(FINISHED_PROCESSING_JOB_MESSAGE_TEMPLATE, job.JobId, job.WorkspaceID, job.TaskType);
				_agentNotifier.NotifyAgent(LogCategory.Info, msg);
				LogFinishingExecuteTask(job);

				return GetTaskResult(job.JobId);
			}
			catch (Exception ex)
			{
				RaiseJobExecutionErrorEvent(job, task, ex);
				return new TaskResult { Status = TaskStatusEnum.Fail, Exceptions = new List<Exception> { ex } };
			}
			finally
			{
				_taskProvider.ReleaseTask(task);
			}
		}

		private void InitializeJobExecution(Job job)
		{
			string agentMessage = string.Format(START_PROCESSING_JOB_MESSAGE_TEMPLATE, job.JobId, job.WorkspaceID,
				job.TaskType);
			_agentNotifier.NotifyAgent(LogCategory.Info, agentMessage);
			LogOnStartJobProcessing(agentMessage, job.JobId, job.WorkspaceID, job.TaskType);
		}

		private void RaiseJobExecutionErrorEvent(Job job, ITask task, Exception exception)
		{
			JobExecutionError?.Invoke(job, task, exception);
		}

		protected TaskResult GetTaskResult(long jobId)
		{
			Job job = _jobService.GetJob(jobId);

			if (job == null)
			{
				throw new InvalidOperationException($"Cannot find job with ID: {jobId}");
			}

			if (job.StopState.HasFlag(StopState.DrainStopping) || job.StopState.HasFlag(StopState.DrainStopped))
            {
				return new TaskResult { Status = TaskStatusEnum.DrainStopped, Exceptions = null };
			}

			return new TaskResult { Status = TaskStatusEnum.Success, Exceptions = null };
		}

		#region Logging

		private void LogJobState(Job job, JobLogState state, Exception exception = null, string details = null)
		{
			if (exception != null)
			{
				details = details ?? string.Empty;
				details += Environment.NewLine;
				details += exception.Message + Environment.NewLine + exception.StackTrace;
			}

			_logger.LogInformation("Integration Points job status update: {@JobLogInformation}", new JobLogInformation
			{
				Job = job.RemoveSensitiveData(), 
				State = state,
				Details = details
			});
		}

		private void LogOnStartJobProcessing(string agentMessage, long nextJobJobId, int nextJobWorkspaceId,
			string nextJobTaskType)
		{
			_logger.LogInformation(agentMessage, nextJobJobId, nextJobWorkspaceId, nextJobTaskType);
		}

		private void LogStartingExecuteTask(Job job)
		{
			_logger.LogInformation("Attempting to execute Job with ID: {JobID} in {TypeName}", job.JobId,
				nameof(ScheduleQueueAgentBase));
		}

		private void LogFinishingExecuteTask(Job job)
		{
			_logger.LogInformation("Finished execution of Job with ID: {JobID} in {TypeName}", job.JobId,
				nameof(ScheduleQueueAgentBase));
		}

		#endregion
	}
}
