using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1.Models.Errors;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class ItemLevelErrorHandler : IItemLevelErrorHandler
    {
        private const int _ITEM_LEVEL_ERRORS_CREATE_BATCH_SIZE = 10000;
        private const string _IDENTIFIER_NOT_FOUND = "[NOT_FOUND]";
        private const string _IAPI_DOCUMENT_IDENTIFIER_COLUMN = "Identifier";

        private readonly IItemLevelErrorHandlerConfiguration _configuration;
        private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
        private readonly IAPILog _log;

        private ConcurrentQueue<CreateJobHistoryErrorDto> _itemErrors = new ConcurrentQueue<CreateJobHistoryErrorDto>();

        public ItemLevelErrorHandler(
            IItemLevelErrorHandlerConfiguration configuration,
            IJobHistoryErrorRepository jobHistoryErrorRepository,
            IAPILog log)
        {
            _configuration = configuration;
            _jobHistoryErrorRepository = jobHistoryErrorRepository;
            _log = log;
        }

        public void HandleItemLevelError(long completedItem, ItemLevelError itemLevelError)
        {
            HandleBatchItemErrorAsync(itemLevelError)
                .GetAwaiter().GetResult();
        }

        public async Task HandleIAPIItemLevelErrorsAsync(ImportErrors errors)
        {
            foreach (var error in errors.Errors.SelectMany(x => x.ErrorDetails))
            {
                ItemLevelError itemLevelError = ToItemLevelError(error);
                await HandleBatchItemErrorAsync(itemLevelError).ConfigureAwait(false);
            }
        }

        public async Task HandleRemainingErrorsAsync()
        {
            if (_itemErrors.Any())
            {
                await CreateErrorsAsync().ConfigureAwait(false);
            }
        }

        private async Task HandleBatchItemErrorAsync(ItemLevelError itemLevelError)
        {
            CreateJobHistoryErrorDto itemError = new CreateJobHistoryErrorDto(ErrorType.Item)
            {
                ErrorMessage = itemLevelError.Message,
                SourceUniqueId = itemLevelError.Identifier
            };

            _itemErrors.Enqueue(itemError);

            if (_itemErrors.Count >= _ITEM_LEVEL_ERRORS_CREATE_BATCH_SIZE)
            {
                await CreateErrorsAsync().ConfigureAwait(false);
            }
        }

        private async Task CreateErrorsAsync()
        {
            int currentNumberOfItemLevelErrorsInQueue = _itemErrors.Count;
            List<CreateJobHistoryErrorDto> itemLevelErrors = new List<CreateJobHistoryErrorDto>(currentNumberOfItemLevelErrorsInQueue);
            for (int i = 0; i < currentNumberOfItemLevelErrorsInQueue; i++)
            {
                if (_itemErrors.TryDequeue(out CreateJobHistoryErrorDto dto))
                {
                    itemLevelErrors.Add(dto);
                }
            }

            if (itemLevelErrors.Any())
            {
                await _jobHistoryErrorRepository.MassCreateAsync(
                        _configuration.SourceWorkspaceArtifactId,
                        _configuration.JobHistoryArtifactId,
                        itemLevelErrors)
                    .ConfigureAwait(false);
            }
        }

        private ItemLevelError ToItemLevelError(ErrorDetail error)
        {
            return new ItemLevelError(
                error.ErrorProperties.TryGetValue(_IAPI_DOCUMENT_IDENTIFIER_COLUMN, out string identifier)
                    ? identifier
                    : _IDENTIFIER_NOT_FOUND,
                error.ErrorMessage);
        }
    }
}
