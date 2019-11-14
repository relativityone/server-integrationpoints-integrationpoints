using System;
using Relativity.Sync.Executors;

namespace Relativity.Sync
{
	internal interface IJobProgressHandler : IDisposable
	{
		int GetBatchItemsProcessedCount(int batchId);

		int GetBatchItemsFailedCount(int batchId);

		IDisposable AttachToImportJob(ISyncImportBulkArtifactJob job, int batchId, int totalItemsInBatch);
	}
}