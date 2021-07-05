using System;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;

namespace Relativity.Sync
{
	internal interface IJobProgressHandler : IDisposable
	{
		IDisposable AttachToImportJob(ISyncImportBulkArtifactJob job, IBatch batch);
	}
}