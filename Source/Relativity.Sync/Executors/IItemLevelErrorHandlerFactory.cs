using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal interface IItemLevelErrorHandlerFactory
    {
        IItemLevelErrorHandler_TEMP CreateBatchItemLevelErrorHandler(IItemStatusMonitor statusMonitor);

        IImportApiItemLevelErrorHandler CreateIApiItemLevelErrorHandler();
    }
}
