using System.Diagnostics.CodeAnalysis;
using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Executors
{
	[ExcludeFromCodeCoverage]
	internal sealed class SyncImportBulkArtifactJob : ISyncImportBulkArtifactJob
	{
		private readonly ImportBulkArtifactJob _importBulkArtifactJob;

		public SyncImportBulkArtifactJob(ImportBulkArtifactJob importBulkArtifactJob)
		{
			_importBulkArtifactJob = importBulkArtifactJob;
		}

		public event IImportNotifier.OnCompleteEventHandler OnComplete
		{
			add => _importBulkArtifactJob.OnComplete += value;
			remove => _importBulkArtifactJob.OnComplete -= value;
		}

		public event IImportNotifier.OnFatalExceptionEventHandler OnFatalException
		{
			add => _importBulkArtifactJob.OnFatalException += value;
			remove => _importBulkArtifactJob.OnFatalException -= value;
		}

		public event IImportNotifier.OnProgressEventHandler OnProgress
		{
			add => _importBulkArtifactJob.OnProgress += value;
			remove => _importBulkArtifactJob.OnProgress -= value;
		}

		public event IImportNotifier.OnProcessProgressEventHandler OnProcessProgress
		{
			add => _importBulkArtifactJob.OnProcessProgress += value;
			remove => _importBulkArtifactJob.OnProcessProgress -= value;
		}

		public event ImportBulkArtifactJob.OnErrorEventHandler OnError
		{
			add => _importBulkArtifactJob.OnError += value;
			remove => _importBulkArtifactJob.OnError -= value;
		}

		public void Execute()
		{
			_importBulkArtifactJob.Execute();
		}
	}
}