﻿using System.Collections.Generic;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.BatchProcess
{
	public abstract class BatchableTaskBase<T> : IBatchableTask<T>
	{
		public BatchableTaskBase()
		{
			BatchSize = 1000;
		}

		public virtual int BatchSize { get; private set; }
		public abstract List<T> GetUnbatchedIDs(Job job);
		public abstract void CreateBatchJob(Job job, List<T> batchIDs);
	}
}
