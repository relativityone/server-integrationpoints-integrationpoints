using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal interface IItemLevelErrorHandler
    {
        void HandleItemLevelError(long completedItem, ItemLevelError itemLevelError);

        Task HandleDataSourceProcessingFinishedAsync(IBatch batch);

        Task HandleIApiItemLevelErrors(
            IImportSourceController sourceController,
            List<IBatch> batches,
            IDocumentSynchronizationMonitorConfiguration configuration);
    }
}
