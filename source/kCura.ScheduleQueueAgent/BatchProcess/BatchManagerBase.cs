using System.Collections.Generic;
using Relativity.API;

namespace kCura.ScheduleQueueAgent.BatchProcess
{
	public abstract class BatchManagerBase<T> : ITask
	{
		public DBContext DBContext { get; private set; }

		public BatchManagerBase(DBContext dbContext)
		{
			DBContext = dbContext;
		}

		public abstract void Execute(Job job);

		public void BatchTask(Job job, IBatchableTask<T> batchableTask)
		{
			List<T> unbatchedItemIDs = batchableTask.GetUnbatchedIDs(job);
			int batchSize = batchableTask.BatchSize;

			if (unbatchedItemIDs.Count > 0)
			{
				int beginIndex = 0;
				while (beginIndex < unbatchedItemIDs.Count)
				{
					int jobBatchSize = (beginIndex + batchSize) >= unbatchedItemIDs.Count
															? unbatchedItemIDs.Count - beginIndex
															: batchSize;
					List<T> batchIDs = unbatchedItemIDs.GetRange(beginIndex, jobBatchSize);
					batchableTask.CreateBatchJob(job, batchIDs);
					beginIndex += jobBatchSize;
				}
			}
		}
	}
}

