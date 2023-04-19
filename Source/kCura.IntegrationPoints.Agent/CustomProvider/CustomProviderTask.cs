using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.Extensions;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.IdFileBuilding;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobDetails;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.SourceProvider;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    internal class CustomProviderTask : ICustomProviderTask
    {
        private readonly IAgentValidator _agentValidator;
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IJobDetailsService _jobDetailsService;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly ISourceProviderService _sourceProviderService;
        private readonly IIdFilesBuilder _idFilesBuilder;
        private readonly ILoadFileBuilder _loadFileBuilder;
        private readonly IRelativityStorageService _relativityStorageService;
        private readonly ISerializer _serializer;
        private readonly IImportApiRunnerFactory _importApiRunnerFactory;
        private readonly IJobProgressHandler _jobProgressHandler;
        private readonly IAPILog _logger;

        public CustomProviderTask(
            IAgentValidator agentValidator,
            IKeplerServiceFactory serviceFactory, 
            IJobDetailsService jobDetailsService,
            IIntegrationPointService integrationPointService,
            ISourceProviderService sourceProviderService,
            IIdFilesBuilder idFilesBuilder,
            ILoadFileBuilder loadFileBuilder,
            IRelativityStorageService relativityStorageService,
            ISerializer serializer,
            IImportApiRunnerFactory importApiRunnerFactory,
            IJobProgressHandler jobProgressHandler,
            IAPILog logger)
        {
            _agentValidator = agentValidator;
            _serviceFactory = serviceFactory;
            _jobDetailsService = jobDetailsService;
            _integrationPointService = integrationPointService;
            _sourceProviderService = sourceProviderService;
            _idFilesBuilder = idFilesBuilder;
            _loadFileBuilder = loadFileBuilder;
            _relativityStorageService = relativityStorageService;
            _serializer = serializer;
            _importApiRunnerFactory = importApiRunnerFactory;
            _jobProgressHandler = jobProgressHandler;
            _logger = logger;
        }

        public void Execute(Job job)
        {
            ExecuteAsync(job).GetAwaiter().GetResult();
        }

        private async Task ExecuteAsync(Job job)
        {
            IntegrationPointDto integrationPointDto = _integrationPointService.Read(job.RelatedObjectArtifactID);

            _agentValidator.Validate(integrationPointDto, job.SubmittedBy);

            CustomProviderJobDetails jobDetails = await _jobDetailsService.GetJobDetailsAsync(job.WorkspaceID, job.JobDetails).ConfigureAwait(false);
            IDataSourceProvider sourceProvider = await _sourceProviderService.GetSourceProviderAsync(job.WorkspaceID, integrationPointDto.SourceProvider);
            ImportSettings destinationConfiguration = _serializer.Deserialize<ImportSettings>(integrationPointDto.DestinationConfiguration);

            DirectoryInfo importDirectory = await _relativityStorageService.PrepareImportDirectoryAsync(job.WorkspaceID, jobDetails.ImportJobID);

            try
            {

                if (!jobDetails.Batches.Any())
                {
                    jobDetails = await CreateBatchesAsync(jobDetails.ImportJobID, job, sourceProvider, integrationPointDto, importDirectory.FullName).ConfigureAwait(false);
                }

                await _jobProgressHandler.SetTotalItemsAsync(job.WorkspaceID, jobDetails.JobHistoryID,
                    jobDetails.Batches.Sum(x => x.NumberOfRecords)).ConfigureAwait(false);

                IImportApiRunner importApiRunner = _importApiRunnerFactory.BuildRunner(destinationConfiguration);

                List<IndexedFieldMap> fieldMapping = IndexFieldMappings(integrationPointDto.FieldMappings);

                var importJobContext = new ImportJobContext(jobDetails.ImportJobID, job.JobId, job.WorkspaceID);
                await importApiRunner.RunImportJobAsync(importJobContext, destinationConfiguration, fieldMapping);
                using (await _jobProgressHandler
                           .BeginUpdateAsync(job.WorkspaceID, jobDetails.ImportJobID, jobDetails.JobHistoryID)
                           .ConfigureAwait(false))
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

                        using (IImportSourceController importSourceController = await _serviceFactory
                                   .CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
                        {
                            Response response = await importSourceController
                                .AddSourceAsync(destinationConfiguration.CaseArtifactId, jobDetails.ImportJobID,
                                    batch.BatchGuid, dataSourceSettings).ConfigureAwait(false);
                            response.Validate();
                        }

                        batch.IsAddedToImportQueue = true;
                        await _jobDetailsService.UpdateJobDetailsAsync(job, jobDetails).ConfigureAwait(false);

                        await _jobProgressHandler.UpdateReadItemsCountAsync(job, jobDetails).ConfigureAwait(false);
                    }
                }

                await EndImportJobAsync(destinationConfiguration.CaseArtifactId, jobDetails.ImportJobID).ConfigureAwait(false);
            }
            catch (ImportApiResponseException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to run import job: {importJobId}. Error code: {errorCode}. Error message: {errorMessage}",
                    ex.Response.ImportJobID,
                    ex.Response.ErrorCode,
                    ex.Response.ErrorMessage);

                await CancelJobAsync(destinationConfiguration.CaseArtifactId, jobDetails.ImportJobID).ConfigureAwait(false);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute Custom Provider job");

                // TODO REL-806942 Mark job as failed
                // There is a newly created method IntegrationPointService.MarkJobAsFailed() (currently private)
                // We can extract this method to the separate service and extend it with other JobHistory(Error) use cases
                await CancelJobAsync(destinationConfiguration.CaseArtifactId, jobDetails.ImportJobID).ConfigureAwait(false);

                throw;
            }
            finally
            {
                await _relativityStorageService.DeleteDirectoryRecursiveAsync(importDirectory.FullName).ConfigureAwait(false);
            }
        }
        
        private async Task EndImportJobAsync(int workspaceId, Guid importJobId)
        {
            try
            {
                using (IImportJobController jobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
                {
                    ValueResponse<ImportDetails> details = await jobController.GetDetailsAsync(workspaceId, importJobId).ConfigureAwait(false);

                    if (details.Value.State != ImportState.Configured)
                    {
                        Response response = await jobController.EndAsync(workspaceId, importJobId).ConfigureAwait(false);
                        response.Validate();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to end import job ID: {importJobId}", importJobId);
                throw;
            }
        }

        private async Task CancelJobAsync(int workspaceId, Guid importJobId)
        {
            try
            {
                using (IImportJobController jobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
                {
                    Response response = await jobController.CancelAsync(workspaceId, importJobId).ConfigureAwait(false);
                    response.Validate();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel import job ID: {importJobId}", importJobId);
                throw;
            }
        }

        private async Task<CustomProviderJobDetails> CreateBatchesAsync(Guid jobId, Job job, IDataSourceProvider provider, IntegrationPointDto integrationPointDto, string importDirectory)
        {
            List<CustomProviderBatch> batches = await _idFilesBuilder.BuildIdFilesAsync(provider, integrationPointDto, importDirectory).ConfigureAwait(false);

            CustomProviderJobDetails jobDetails = new CustomProviderJobDetails()
            {
                ImportJobID = jobId,
                Batches = batches
            };

            await _jobDetailsService.UpdateJobDetailsAsync(job, jobDetails).ConfigureAwait(false);

            return jobDetails;
        }
        
        private static List<IndexedFieldMap> IndexFieldMappings(List<FieldMap> fieldMappings)
        {
            return fieldMappings.Select((map, i) => new IndexedFieldMap(map, i)).ToList();
        }
    }
}
