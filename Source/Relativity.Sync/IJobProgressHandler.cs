using System;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal interface IJobProgressHandler : IDisposable
	{
		int GetBatchItemsProcessedCount(int batchId);

		int GetBatchItemsFailedCount(int batchId);

		IDisposable AttachToImportJob(ISyncImportBulkArtifactJob job, IBatch batch);
	}
}