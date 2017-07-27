using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.BatchProcess
{
	public delegate void JobPreExecuteEvent(Job job);

	public delegate void JobPostExecuteEvent(Job job, TaskResult taskResult, int items);

	public abstract class BatchManagerBase<T> : ITask
	{
		private readonly IAPILog _logger;

		protected BatchManagerBase(IHelper helper)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<BatchManagerBase<T>>();
			BatchSize = 1000;
		}

		public virtual int BatchSize { get; }

		public virtual void Execute(Job job)
		{
		    LogExecuteStart(job);

            TaskResult taskResult = new TaskResult();
			int items = 0;
			try
			{
				OnRaiseJobPreExecute(job);
				items = BatchTask(job, GetUnbatchedIDs(job));
				taskResult.Status = TaskStatusEnum.Success;
			    LogExecuteSuccesfulEnd(job);
            }
			catch (OperationCanceledException e)
			{
				taskResult.Status = TaskStatusEnum.Success;
				LogStoppingJob(e);
				// DO NOTHING. Someone attempted to stop the job.
			}
			catch (Exception e)
			{
				taskResult.Status = TaskStatusEnum.Fail;
				taskResult.Exceptions = new List<Exception> {e};
				LogJobFailed(e);
				throw;
			}
			finally
			{
			    OnRaiseJobPostExecute(job, taskResult, items);
			    LogExecuteFinalize(job);
			}
		}


	    public event JobPreExecuteEvent RaiseJobPreExecute;
		public event JobPostExecuteEvent RaiseJobPostExecute;

		protected virtual void OnRaiseJobPreExecute(Job job)
		{
			if (RaiseJobPreExecute != null)
			{
			    LogRaisePreExecute(job);
			    RaiseJobPreExecute(job);
			}
		}

	    protected virtual void OnRaiseJobPostExecute(Job job, TaskResult taskResult, int items)
		{
			if (RaiseJobPostExecute != null)
			{
			    LogRaisePostExecute(job);
			    RaiseJobPostExecute(job, taskResult, items);
			}
		}

        public abstract IEnumerable<T> GetUnbatchedIDs(Job job);

		public virtual int BatchTask(Job job, IEnumerable<T> batchIDs)
		{
			int count = 0;
			var list = new List<T>();
			foreach (var id in batchIDs)
			{
				//TODO: later we will need to generate error entry for every item we bypass
				if ((id != null) && id is string && (id.ToString() != string.Empty))
				{
					list.Add(id);
					count += 1;
					if (list.Count == BatchSize)
					{
						CreateBatchJob(job, list);
						list = new List<T>();
					}
				}
			}
			if (list.Any())
			{
				CreateBatchJob(job, list);
			}
			return count;
		}

		public abstract void CreateBatchJob(Job job, List<T> batchIDs);

		#region Logging

		private void LogJobFailed(Exception e)
		{
			_logger.LogError(e, "Failed to execute job");
		}

		private void LogStoppingJob(OperationCanceledException e)
		{
			_logger.LogInformation(e, "Someone attempted to stop the job");
		}

	    private void LogExecuteFinalize(Job job)
	    {
	        _logger.LogInformation("Batch Manager Base: Finalizing execution of job: {JobId}", job.JobId);
	    }

	    private void LogExecuteSuccesfulEnd(Job job)
	    {
	        _logger.LogInformation("Batch Manager Base: Succesfully executed job: {JobId}", job.JobId);
	    }

	    private void LogExecuteStart(Job job)
	    {
	        _logger.LogInformation("Batch Manager Base: Started execution of job: {JobId}", job.JobId);
	    }

	    private void LogRaisePostExecute(Job job)
	    {
	        _logger.LogInformation("Batch Manager Base: Raising post execute event for job: {JobId}", job.JobId);
	    }

	    private void LogRaisePreExecute(Job job)
	    {
	        _logger.LogInformation("Batch Manager Base: Raising pre execute event for job: {JobId}", job.JobId);
	    }

        #endregion
    }
}