using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Executors
{
	internal interface ISyncImportBulkArtifactJob : IImportNotifier
	{
		event ImportBulkArtifactJob.OnErrorEventHandler OnError;
		void Execute();
	}
}