using System.Collections.Generic;
using Relativity.API;

namespace kCura.ScheduleQueueAgent.BatchProcess
{
	public abstract class BatchableTaskBase<T> : IBatchableTask<T>
	{
		public BatchableTaskBase(DBContext dbContext, int batchSize = 1000)
		{
			DBContext = dbContext;
			BatchSize = batchSize;
		}

		public DBContext DBContext { get; private set; }
		public virtual int BatchSize { get; private set; }
		public abstract List<T> GetUnbatchedIDs(Job job);
		public abstract void CreateBatchJob(Job job, List<T> batchIDs);
	}
}
