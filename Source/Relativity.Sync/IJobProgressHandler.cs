using System;
using Relativity.Sync.Executors;

namespace Relativity.Sync
{
	internal interface IJobProgressHandler : IDisposable
	{
		IDisposable AttachToImportJob(ISyncImportBulkArtifactJob job, int batchId, int totalItemsInBatch);
	}
}