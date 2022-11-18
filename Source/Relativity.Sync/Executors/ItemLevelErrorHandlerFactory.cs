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

        public IItemLevelErrorHandler Create(IItemStatusMonitor statusMonitor)
        {
            ItemLevelErrorHandler itemLevelErrorHandler = new ItemLevelErrorHandler(_configuration, _jobHistoryErrorRepository, _logger);
            itemLevelErrorHandler.Initialize(statusMonitor);
            return itemLevelErrorHandler;
        }
    }
}
