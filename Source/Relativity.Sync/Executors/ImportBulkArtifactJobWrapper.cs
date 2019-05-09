using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Executors
{
	internal sealed class ImportBulkArtifactJobWrapper : IImportBulkArtifactJob
	{
		private readonly ImportBulkArtifactJob _importBulkArtifactJob;

		public ImportBulkArtifactJobWrapper(ImportBulkArtifactJob importBulkArtifactJob)
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

		public void Execute()
		{
			_importBulkArtifactJob.Execute();
		}
	}
}