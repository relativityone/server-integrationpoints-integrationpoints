using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage.ImportApiService;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.FieldMapping;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobDetails;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding;
using kCura.IntegrationPoints.Agent.CustomProvider.Utils;
using kCura.IntegrationPoints.Core.Storage;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Sources;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Sync;
using BatchStatus = kCura.IntegrationPoints.Agent.CustomProvider.DTO.BatchStatus;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal class ImportJobRunner : IImportJobRunner
    {
        internal TimeSpan WaitForJobToFinishInterval = TimeSpan.FromSeconds(1);

        private readonly IImportApiService _importApiService;
        private readonly IJobDetailsService _jobDetailsService;
        private readonly ILoadFileBuilder _loadFileBuilder;
        private readonly IRelativityStorageService _relativityStorageService;
        private readonly IImportApiRunnerFactory _importApiRunnerFactory;
        private readonly IJobProgressHandler _jobProgressHandler;
        private readonly IItemLevelErrorHandler _errorsHandler;
        private readonly IFieldMapService _fieldMapService;

        private readonly IAPILog _logger;

        public ImportJobRunner(
            IImportApiService importApiService,
            IJobDetailsService jobDetailsService,
            ILoadFileBuilder loadFileBuilder,
            IRelativityStorageService relativityStorageService,
            IImportApiRunnerFactory importApiRunnerFactory,
            IJobProgressHandler jobProgressHandler,
            IItemLevelErrorHandler errorsHandler,
            IFieldMapService fieldMapService,
            IAPILog logger)
        {
            _importApiService = importApiService;
            _jobDetailsService = jobDetailsService;
            _loadFileBuilder = loadFileBuilder;
            _relativityStorageService = relativityStorageService;
            _importApiRunnerFactory = importApiRunnerFactory;
            _jobProgressHandler = jobProgressHandler;
            _errorsHandler = errorsHandler;
            _fieldMapService = fieldMapService;
            _logger = logger;
        }

        public async Task<ImportJobResult> RunJobAsync(Job job, CustomProviderJobDetails jobDetails, IntegrationPointInfo integrationPointInfo, ImportJobContext importJobContext, IDataSourceProvider sourceProvider, CompositeCancellationToken token)
        {
            DirectoryInfo importDirectory = await _relativityStorageService.PrepareImportDirectoryAsync(job.WorkspaceID, jobDetails.JobHistoryGuid);
            try
            {
                IImportApiRunner importApiRunner = _importApiRunnerFactory.BuildRunner(integrationPointInfo.DestinationConfiguration);

                IndexedFieldMap identifierField = await _fieldMapService
                    .GetIdentifierFieldAsync(integrationPointInfo.DestinationConfiguration.CaseArtifactId,
                        integrationPointInfo.DestinationConfiguration.ArtifactTypeId, integrationPointInfo.FieldMap)
                    .ConfigureAwait(false);

                await importApiRunner.RunImportJobAsync(
                        importJobContext,
                        integrationPointInfo,
                        identifierField)
                    .ConfigureAwait(false);

                using (await _jobProgressHandler.BeginUpdateAsync(importJobContext).ConfigureAwait(false))
                {
                    foreach (CustomProviderBatch batch in jobDetails.Batches)
                    {
                        if (batch.IsAddedToImportQueue)
                        {
                            continue;
                        }

                        await AddBatchToImportDataSourceAsync(
                                batch,
                                jobDetails,
                                sourceProvider,
                                integrationPointInfo,
                                importDirectory,
                                importJobContext,
                                job)
                            .ConfigureAwait(false);
                    }

                    await _importApiService.EndJobAsync(importJobContext).ConfigureAwait(false);

                    ImportJobResult importJobResult = await WaitForJobToFinishAsync(job, importJobContext, jobDetails, token).ConfigureAwait(false);

                    await _jobProgressHandler.SafeUpdateProgressAsync(importJobContext).ConfigureAwait(false);

                    return importJobResult;
                }
            }
            catch (ImportApiResponseException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to run import job: {importJobId}. Error code: {errorCode}. Error message: {errorMessage}",
                    ex.Response.ImportJobID,
                    ex.Response.ErrorCode,
                    ex.Response.ErrorMessage);

                await _importApiService.CancelJobAsync(importJobContext).ConfigureAwait(false);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute Custom Provider job");

                await _importApiService.CancelJobAsync(importJobContext).ConfigureAwait(false);

                throw;
            }
            finally
            {
                if (!token.IsDrainStopRequested)
                {
                    await _relativityStorageService.DeleteDirectoryRecursiveAsync(importDirectory.FullName).ConfigureAwait(false);
                }
            }
        }

        private async Task AddBatchToImportDataSourceAsync(
            CustomProviderBatch batch,
            CustomProviderJobDetails jobDetails,
            IDataSourceProvider sourceProvider,
            IntegrationPointInfo integrationPointInfo,
            DirectoryInfo importDirectory,
            ImportJobContext importJobContext,
            Job job)
        {
            DataSourceSettings dataSourceSettings = await _loadFileBuilder.CreateDataFileAsync(
                    batch,
                    sourceProvider,
                    integrationPointInfo,
                    importDirectory.FullName)
                .ConfigureAwait(false);

            await _importApiService.AddDataSourceAsync(importJobContext, batch.BatchGuid, dataSourceSettings).ConfigureAwait(false);
            batch.IsAddedToImportQueue = true;
            await _jobDetailsService.UpdateJobDetailsAsync(job, jobDetails).ConfigureAwait(false);
            await _jobProgressHandler.UpdateReadItemsCountAsync(job, jobDetails).ConfigureAwait(false);
        }

        private async Task<ImportJobResult> WaitForJobToFinishAsync(Job job, ImportJobContext importJobContext, CustomProviderJobDetails details, CompositeCancellationToken token)
        {
            ImportDetails result;
            ImportState state = ImportState.Unknown;
            do
            {
                if (token.IsStopRequested)
                {
                    await _importApiService.CancelJobAsync(importJobContext).ConfigureAwait(false);
                    new ImportJobResult { Status = JobEndStatus.Canceled };
                }

                if (token.IsDrainStopRequested)
                {
                    new ImportJobResult { Status = JobEndStatus.DrainStopped };
                }

                await Task.Delay(WaitForJobToFinishInterval).ConfigureAwait(false);

                result = await _importApiService.GetJobImportStatusAsync(importJobContext).ConfigureAwait(false);
                if (result.State != state)
                {
                    state = result.State;
                    _logger.LogInformation("Import status: {@status}", result);
                }

                await HandleDataSourceStatusAsync(job, importJobContext, details).ConfigureAwait(false);
            }
            while (!result.IsFinished);

            return ImportJobResult.FromImportDetails(result);
        }

        private async Task HandleDataSourceStatusAsync(Job job, ImportJobContext importJobContext, CustomProviderJobDetails details)
        {
            foreach (var batch in details.Batches.Where(x => x.Status == BatchStatus.Started))
            {
                DataSourceDetails dataSourceDetails = await _importApiService.GetDataSourceDetailsAsync(importJobContext, batch.BatchGuid).ConfigureAwait(false);
                if (dataSourceDetails.IsFinished())
                {
                    _logger.LogInformation("DataSource {dataSource} has finished with status {dataSourceState}.", batch.BatchGuid, dataSourceDetails.State);
                    switch (dataSourceDetails.State)
                    {
                        case DataSourceState.Completed:
                            batch.Status = BatchStatus.Completed;
                            break;
                        case DataSourceState.CompletedWithItemErrors:
                            batch.Status = BatchStatus.CompletedWithErrors;
                            await _errorsHandler.HandleItemErrorsAsync(importJobContext, batch).ConfigureAwait(false);
                            break;
                        case DataSourceState.Failed:
                            batch.Status = BatchStatus.Failed;
                            break;
                    }

                    await _jobDetailsService.UpdateJobDetailsAsync(job, details).ConfigureAwait(false);
                }
            }
        }
    }
}
