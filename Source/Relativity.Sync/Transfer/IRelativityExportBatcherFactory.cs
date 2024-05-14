using Relativity.Sync.Kepler.SyncBatch;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Transfer
{
    internal interface IRelativityExportBatcherFactory
    {
        IRelativityExportBatcher CreateRelativityExportBatcher(IBatch batch);

        IRelativityExportBatcher CreateRelativityExportBatchForTagging(SyncBatchDto batch);
    }
}
