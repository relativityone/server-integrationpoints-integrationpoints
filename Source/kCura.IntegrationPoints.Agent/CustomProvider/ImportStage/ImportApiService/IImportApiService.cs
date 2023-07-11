using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Errors;
using Relativity.Import.V1.Models.Sources;

namespace kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService
{
    /// <summary>
    /// An interface for ImportAPI v2.0 kepler calls.
    /// </summary>
    internal interface IImportApiService
    {
        /// <summary>
        /// Calls the ImportAPI service to create the import job.
        /// </summary>
        /// <param name="importJobContext">The job context containing the job-id and correlation-id.</param>
        /// <exception cref="ImportApiResponseException">Thrown when ImportAPI call fails.</exception>
        Task CreateImportJobAsync(ImportJobContext importJobContext);

        /// <summary>
        /// Calls the ImportAPI service to begin the import job.
        /// </summary>
        /// <param name="importJobContext">The job context containing the job-id and correlation-id.</param>
        /// <exception cref="ImportApiResponseException">Thrown when ImportAPI call fails.</exception>
        Task StartImportJobAsync(ImportJobContext importJobContext);

        /// <summary>
        /// Calls the ImportAPI service to configure the job with document flow.
        /// </summary>
        /// <param name="importJobContext">The job context containing the job-id and correlation-id.</param>
        /// <param name="configuration">The configuration specific for document flow.</param>
        /// <exception cref="ImportApiResponseException">Thrown when ImportAPI call fails.</exception>
        Task ConfigureDocumentImportApiJobAsync(ImportJobContext importJobContext, DocumentImportConfiguration configuration);

        /// <summary>
        /// Calls the ImportAPI service to configure the job with rdo flow.
        /// </summary>
        /// <param name="importJobContext">The job context containing the job-id and correlation-id.</param>
        /// <param name="configuration">The configuration specific for rdo flow.</param>
        /// <exception cref="ImportApiResponseException">Thrown when ImportAPI call fails.</exception>
        Task ConfigureRdoImportApiJobAsync(ImportJobContext importJobContext, RdoImportConfiguration configuration);

        Task AddDataSourceAsync(ImportJobContext importJobContext, Guid sourceId, DataSourceSettings dataSourceSettings);

        Task CancelJobAsync(ImportJobContext importJobContext);

        Task EndJobAsync(ImportJobContext importJobContext);

        Task<ImportProgress> GetJobImportProgressValueAsync(ImportJobContext importJobContext);

        Task<ImportDetails> GetJobImportStatusAsync(ImportJobContext importJobContext);

        Task<DataSourceDetails> GetDataSourceDetailsAsync(ImportJobContext importJobContext, Guid sourceId);

        Task<ImportErrors> GetDataSourceErrorsAsync(ImportJobContext importJobContext, Guid sourceId, int length);
    }
}
