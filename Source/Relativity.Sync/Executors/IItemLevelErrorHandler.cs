using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Import.V1.Models.Errors;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal interface IItemLevelErrorHandler_TEMP
    {
        void HandleItemLevelError(long completedItem, ItemLevelError itemLevelError);

        Task HandleDataSourceProcessingFinishedAsync(IBatch batch);
    }

    internal interface IItemLevelErrorHandler
    {
        void HandleItemLevelError(long completedItem, ItemLevelError itemLevelError);

        void HandleIAPIItemLevelErrors(IEnumerable<ImportErrors> errors);

        Task HandleRemainingErrorsAsync();
    }
}
