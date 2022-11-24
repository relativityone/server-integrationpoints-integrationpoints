using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal interface IItemLevelErrorHandlerFactory
    {
        IItemLevelErrorHandler Create(IItemStatusMonitor statusMonitor);

        IImportApiItemLevelErrorHandler CreateIApiHandler();
    }
}