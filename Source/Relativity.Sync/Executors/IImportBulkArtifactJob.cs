using kCura.Relativity.DataReaderClient;

namespace Relativity.Sync.Executors
{
	internal interface IImportBulkArtifactJob : IImportNotifier
	{
		void Execute();
	}
}