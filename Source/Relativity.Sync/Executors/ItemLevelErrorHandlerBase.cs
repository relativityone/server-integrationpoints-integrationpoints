using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal abstract class ItemLevelErrorHandlerBase
    {
        private const int _BATCH_ITEM_ERRORS_COUNT_FOR_RDO_CREATE = 1000;

        private readonly IItemLevelErrorHandlerConfiguration _configuration;
        private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;

        protected ItemLevelErrorHandlerBase(IItemLevelErrorHandlerConfiguration configuration, IJobHistoryErrorRepository jobHistoryErrorRepository)
        {
            _configuration = configuration;
            _jobHistoryErrorRepository = jobHistoryErrorRepository;
            BatchItemErrors = new ConcurrentQueue<CreateJobHistoryErrorDto>();
        }

        protected ConcurrentQueue<CreateJobHistoryErrorDto> BatchItemErrors { get; }

        protected void HandleBatchItemErrors(ItemLevelError itemLevelError)
        {
            CreateJobHistoryErrorDto itemError = new CreateJobHistoryErrorDto(ErrorType.Item)
            {
                ErrorMessage = itemLevelError.Message,
                SourceUniqueId = itemLevelError.Identifier
            };

            BatchItemErrors.Enqueue(itemError);

            if (BatchItemErrors.Count >= _BATCH_ITEM_ERRORS_COUNT_FOR_RDO_CREATE)
            {
                CreateJobHistoryErrors();
            }
        }

        protected void CreateJobHistoryErrors()
        {
            int currentNumberOfItemLevelErrorsInQueue = BatchItemErrors.Count;
            List<CreateJobHistoryErrorDto> itemLevelErrors = new List<CreateJobHistoryErrorDto>(currentNumberOfItemLevelErrorsInQueue);
            for (int i = 0; i < currentNumberOfItemLevelErrorsInQueue; i++)
            {
                if (BatchItemErrors.TryDequeue(out CreateJobHistoryErrorDto dto))
                {
                    itemLevelErrors.Add(dto);
                }
            }

            if (itemLevelErrors.Any())
            {
                _jobHistoryErrorRepository.MassCreateAsync(_configuration.SourceWorkspaceArtifactId, _configuration.JobHistoryArtifactId, itemLevelErrors)
                    .GetAwaiter()
                    .GetResult();
            }
        }
    }
}
