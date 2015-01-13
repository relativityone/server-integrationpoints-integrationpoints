using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.ScheduleQueue.Core.BatchProcess
{
	public delegate void JobPreExecuteEvent(Job job);
	public delegate void JobPostExecuteEvent(Job job, TaskResult taskResult);

	public abstract class BatchManagerBase<T> : ITask
	{
		public event JobPreExecuteEvent RaiseJobPreExecute;
		public event JobPostExecuteEvent RaiseJobPostExecute;

		protected BatchManagerBase()
		{
			BatchSize = 1000;
		}

		public virtual void Execute(Job job)
		{
			TaskResult taskResult = new TaskResult();
			try
			{
				OnRaiseJobPreExecute(job);
				BatchTask(job, GetUnbatchedIDs(job));
				taskResult.Status = TaskStatusEnum.Success;
			}
			catch (Exception e)
			{
				taskResult.Status = TaskStatusEnum.Fail;
				taskResult.Exceptions = new List<Exception>() { e };
				throw;
			}
			finally
			{
				OnRaiseJobPostExecute(job, taskResult);
			}
		}

		protected virtual void OnRaiseJobPreExecute(Job job)
		{
			if (RaiseJobPreExecute != null)
				RaiseJobPreExecute(job);
		}

		protected virtual void OnRaiseJobPostExecute(Job job, TaskResult taskResult)
		{
			if (RaiseJobPostExecute != null)
				RaiseJobPostExecute(job, taskResult);
		}

		public virtual int BatchSize { get; private set; }

		public abstract IEnumerable<T> GetUnbatchedIDs(Job job);

		public virtual void BatchTask(Job job, IEnumerable<T> batchIDs)
		{
			var list = new List<T>();
			var idx = 0;
			foreach (var id in batchIDs)
			{
				list.Add(id);
				if (idx >= BatchSize)
				{
					CreateBatchJob(job, list);
					list = new List<T>();
					idx = 0;
				}
				idx++;
			}
			if (list.Any())
			{
				CreateBatchJob(job, list);
			}
		}
		public abstract void CreateBatchJob(Job job, List<T> batchIDs);
	}
}

