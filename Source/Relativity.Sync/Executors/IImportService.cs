using System;
using System.Threading.Tasks;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Errors;
using Relativity.Import.V1.Models.Sources;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Executors
{
    internal interface IImportService
    {
        Task CreateImportJobAsync(SyncJobParameters parameters);

        Task ConfigureDocumentImportSettingsAsync(ImportSettings settings);

        Task BeginImportJobAsync();

        Task AddDataSourceAsync(Guid batchGuid, DataSourceSettings settings);

        Task CancelJobAsync();

        Task EndJobAsync();

        Task<ImportProgress> GetJobImportProgressValueAsync();

        Task<ImportDetails> GetJobImportStatusAsync();

        Task<ImportErrors> GetDataSourceErrorsAsync(Guid dataSourceId, int start, int length);

        Task<DataSourceDetails> GetDataSourceStatusAsync(Guid dataSourceGuid);

        Task<ImportProgress> GetDataSourceProgressAsync(Guid dataSourceGuid);
    }
}
