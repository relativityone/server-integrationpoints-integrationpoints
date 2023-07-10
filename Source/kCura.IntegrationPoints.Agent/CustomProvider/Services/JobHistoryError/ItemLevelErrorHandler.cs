using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using Relativity.Import.V1.Models.Errors;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError
{
    internal class ItemLevelErrorHandler : IItemLevelErrorHandler
    {
        private const string _IDENTIFIER_NOT_FOUND = "[NOT_FOUND]";
        private const string _IAPI_DOCUMENT_IDENTIFIER_COLUMN = "Identifier";

        private readonly IImportApiService _importApiService;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;

        public ItemLevelErrorHandler(IImportApiService importApiService, IJobHistoryErrorService jobHistoryErrorService)
        {
            _importApiService = importApiService;
            _jobHistoryErrorService = jobHistoryErrorService;
        }

        public async Task HandleItemErrorsAsync(ImportJobContext importJobContext, CustomProviderBatch batch)
        {
            await HandleDataSourceErrorsAsync(importJobContext, batch.BatchGuid, batch.NumberOfRecords).ConfigureAwait(false);
        }

        private async Task HandleDataSourceErrorsAsync(ImportJobContext importJobContext, Guid dataSourceId, int recordsCount)
        {
            ImportErrors errors = await _importApiService.GetDataSourceErrorsAsync(importJobContext, dataSourceId, recordsCount).ConfigureAwait(false);

            List<ItemLevelError> itemLevelErrors = errors.Errors.SelectMany(x => x.ErrorDetails).Select(x => ToItemLevelError(x)).ToList();

            await _jobHistoryErrorService.CreateItemLevelErrorsAsync(importJobContext.WorkspaceId, importJobContext.JobHistoryId, itemLevelErrors);
        }

        private ItemLevelError ToItemLevelError(ErrorDetail error)
        {
            string uniqueId = error.ErrorProperties.TryGetValue(_IAPI_DOCUMENT_IDENTIFIER_COLUMN, out string identifier)
                ? identifier
                : _IDENTIFIER_NOT_FOUND;

            return new ItemLevelError()
            {
                ErrorMessage = error.ErrorMessage,
                SourceUniqueId = uniqueId
            };
        }
    }
}
