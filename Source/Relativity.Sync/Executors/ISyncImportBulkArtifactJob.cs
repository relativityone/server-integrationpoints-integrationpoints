using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal interface ISyncImportBulkArtifactJob : IImportNotifier
	{
		IItemStatusMonitor ItemStatusMonitor { get; }

		event ImportBulkArtifactJob.OnErrorEventHandler OnError;

		void Execute();
	}
}