using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal delegate void OnSyncImportBulkArtifactJobItemLevelErrorEventHandler(ItemLevelError itemLevelError);

	internal interface ISyncImportBulkArtifactJob : IImportNotifier
	{
		IItemStatusMonitor ItemStatusMonitor { get; }

		event OnSyncImportBulkArtifactJobItemLevelErrorEventHandler OnItemLevelError;

		void Execute();
	}
}