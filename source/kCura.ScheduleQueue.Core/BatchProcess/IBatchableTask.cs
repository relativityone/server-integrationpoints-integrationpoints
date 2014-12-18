using System.Collections.Generic;

namespace kCura.ScheduleQueue.Core.BatchProcess
{
	public interface IBatchableTask<T>
	{
		int BatchSize { get; }
		List<T> GetUnbatchedIDs(Job job);
		void CreateBatchJob(Job job, List<T> batchIDs);
	}
}
