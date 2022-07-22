using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Executors
{
    internal delegate void SyncJobEventHandler<T>(T argument);

    internal interface ISyncImportBulkArtifactJob
    {
        IItemStatusMonitor ItemStatusMonitor { get; }

        event SyncJobEventHandler<ItemLevelError> OnItemLevelError;
        event SyncJobEventHandler<ImportApiJobProgress> OnProgress; 
        event SyncJobEventHandler<ImportApiJobStatistics> OnComplete;
        event SyncJobEventHandler<ImportApiJobStatistics> OnFatalException;

        void Execute();
    }
}