using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class ItemLevelErrorHandlerFactory : IItemLevelErrorHandlerFactory
    {
        private readonly IItemLevelErrorHandlerConfiguration _configuration;
        private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
        private readonly IAPILog _logger;

        public ItemLevelErrorHandlerFactory(IItemLevelErrorHandlerConfiguration configuration, IJobHistoryErrorRepository jobHistoryErrorRepository, IAPILog logger)
        {
            _configuration = configuration;
            _jobHistoryErrorRepository = jobHistoryErrorRepository;
            _logger = logger;
        }

        public IItemLevelErrorHandler_TEMP CreateBatchItemLevelErrorHandler(IItemStatusMonitor statusMonitor)
        {
            ItemLevelErrorHandler_TEMP itemLevelErrorHandler = new ItemLevelErrorHandler_TEMP(_configuration, _jobHistoryErrorRepository, statusMonitor, _logger);
            return itemLevelErrorHandler;
        }

        public IImportApiItemLevelErrorHandler CreateIApiItemLevelErrorHandler()
        {
            ImportApiItemLevelErrorHandler itemLevelErrorHandler = new ImportApiItemLevelErrorHandler(_configuration, _jobHistoryErrorRepository);
            return itemLevelErrorHandler;
        }
    }
}
