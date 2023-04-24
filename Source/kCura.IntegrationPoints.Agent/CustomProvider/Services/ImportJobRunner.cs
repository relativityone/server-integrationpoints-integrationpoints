using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.IdFileBuilding;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobDetails;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Import.V1.Models.Sources;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services
{
    internal class ImportJobRunner : IImportJobRunner
    {
        private readonly IImportApiService _importApiService;
        private readonly IJobDetailsService _jobDetailsService;
        private readonly IIdFilesBuilder _idFilesBuilder;
        private readonly ILoadFileBuilder _loadFileBuilder;
        private readonly IRelativityStorageService _relativityStorageService;
        private readonly IImportApiRunnerFactory _importApiRunnerFactory;
        private readonly IJobProgressHandler _jobProgressHandler;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IAPILog _logger;

        public ImportJobRunner(IImportApiService importApiService, IJobDetailsService jobDetailsService, IIdFilesBuilder idFilesBuilder, ILoadFileBuilder loadFileBuilder,
            IRelativityStorageService relativityStorageService, IImportApiRunnerFactory importApiRunnerFactory, IJobProgressHandler jobProgressHandler, IJobHistoryService jobHistoryService, IAPILog logger)
        {
            _importApiService = importApiService;
            _jobDetailsService = jobDetailsService;
            _idFilesBuilder = idFilesBuilder;
            _loadFileBuilder = loadFileBuilder;
            _relativityStorageService = relativityStorageService;
            _importApiRunnerFactory = importApiRunnerFactory;
            _jobProgressHandler = jobProgressHandler;
            _jobHistoryService = jobHistoryService;
            _logger = logger;
        }

        public async Task RunJobAsync(Job job, CustomProviderJobDetails jobDetails, IntegrationPointDto integrationPointDto, IDataSourceProvider sourceProvider, ImportSettings destinationConfiguration, CompositeCancellationToken token)
        {
            var importJobContext = new ImportJobContext(job.WorkspaceID, job.JobId, jobDetails.JobHistoryGuid, jobDetails.JobHistoryID);

            DirectoryInfo importDirectory = await _relativityStorageService.PrepareImportDirectoryAsync(job.WorkspaceID, jobDetails.JobHistoryGuid);

            try
            {
                if (!jobDetails.Batches.Any())
                {
                    jobDetails.Batches = await CreateBatchesAsync(sourceProvider, integrationPointDto, importDirectory.FullName).ConfigureAwait(false);
                    await _jobDetailsService.UpdateJobDetailsAsync(job, jobDetails).ConfigureAwait(false);
                }

                int totalItemsCount = jobDetails.Batches.Sum(x => x.NumberOfRecords);
                await _jobProgressHandler.SetTotalItemsAsync(job.WorkspaceID, jobDetails.JobHistoryID, totalItemsCount).ConfigureAwait(false);

                IImportApiRunner importApiRunner = _importApiRunnerFactory.BuildRunner(destinationConfiguration);

                List<IndexedFieldMap> fieldMapping = IndexFieldMappings(integrationPointDto.FieldMappings);

                await importApiRunner.RunImportJobAsync(importJobContext, destinationConfiguration, fieldMapping);

                using (await _jobProgressHandler.BeginUpdateAsync(importJobContext).ConfigureAwait(false))
                {
                    foreach (CustomProviderBatch batch in jobDetails.Batches)
                    {
                        if (batch.IsAddedToImportQueue)
                        {
                            continue;
                        }

                        DataSourceSettings dataSourceSettings = await _loadFileBuilder.CreateDataFileAsync(
                                batch,
                                sourceProvider,
                                new IntegrationPointInfo()
                                {
                                    SourceConfiguration = integrationPointDto.SourceConfiguration,
                                    SecuredConfiguration = integrationPointDto.SecuredConfiguration,
                                    FieldMap = fieldMapping
                                },
                                importDirectory.FullName)
                            .ConfigureAwait(false);

                        await _importApiService.AddDataSourceAsync(importJobContext, batch.BatchGuid, dataSourceSettings).ConfigureAwait(false);
                        batch.IsAddedToImportQueue = true;
                        await _jobDetailsService.UpdateJobDetailsAsync(job, jobDetails).ConfigureAwait(false);
                        await _jobProgressHandler.UpdateReadItemsCountAsync(job, jobDetails).ConfigureAwait(false);
                    }

                    await _jobProgressHandler.WaitForJobToFinish(importJobContext, token).ConfigureAwait(false);
                    await _jobProgressHandler.SafeUpdateProgressAsync(importJobContext).ConfigureAwait(false);
                }

                await _importApiService.EndJobAsync(importJobContext).ConfigureAwait(false);
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
                await _jobHistoryService.UpdateStatusAsync(job.WorkspaceID, jobDetails.JobHistoryID,
                    JobStatusChoices.JobHistoryErrorJobFailedGuid).ConfigureAwait(false);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute Custom Provider job");

                await _importApiService.CancelJobAsync(importJobContext).ConfigureAwait(false);
                await _jobHistoryService.UpdateStatusAsync(job.WorkspaceID, jobDetails.JobHistoryID,
                    JobStatusChoices.JobHistoryErrorJobFailedGuid).ConfigureAwait(false);

                throw;
            }
            finally
            {
                await _relativityStorageService.DeleteDirectoryRecursiveAsync(importDirectory.FullName)
                    .ConfigureAwait(false);
            }
        }
        
        private async Task<List<CustomProviderBatch>> CreateBatchesAsync(IDataSourceProvider provider, IntegrationPointDto integrationPointDto, string importDirectory)
        {
            List<CustomProviderBatch> batches = await _idFilesBuilder.BuildIdFilesAsync(provider, integrationPointDto, importDirectory).ConfigureAwait(false);
            return batches;
        }

        private static List<IndexedFieldMap> IndexFieldMappings(List<FieldMap> fieldMappings)
        {
            return fieldMappings.Select((map, i) => new IndexedFieldMap(map, i)).ToList();
        }
    }
}