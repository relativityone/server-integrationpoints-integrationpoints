using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Executors
{
	internal interface IImportBulkArtifactJob : IImportNotifier
	{
		event ImportBulkArtifactJob.OnErrorEventHandler OnError;
		void Execute();
	}
}