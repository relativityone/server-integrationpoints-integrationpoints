using System.Collections.Generic;

namespace kCura.Agent.ScheduleQueueAgent.BatchProcess
{
	public interface IBatchableTask<T>
	{
		int BatchSize { get; }
		List<T> GetUnbatchedIDs(Job job);
		void CreateBatchJob(Job job, List<T> batchIDs);
	}
}
