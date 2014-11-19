using System.Collections.Generic;
using System.Linq;
using Relativity.API;

namespace kCura.ScheduleQueueAgent.BatchProcess
{
	public abstract class BatchManagerBase<T> : ITask
	{
		protected BatchManagerBase()
		{
			BatchSize = 1000;
		}

		public virtual void Execute(Job job)
		{
			BatchTask(job, GetUnbatchedIDs(job));
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

