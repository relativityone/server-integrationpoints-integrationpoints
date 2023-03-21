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
using kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.Interfaces;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    internal class CustomProviderTask : ICustomProviderTask
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly ISourceProviderService _sourceProviderService;
        private readonly IIdFilesBuilder _idFilesBuilder;
        private readonly ILoadFileBuilder _loadFileBuilder;
        private readonly IRelativityStorageService _relativityStorageService;
        private readonly ISerializer _serializer;
        private readonly IJobService _jobService;
        private readonly IImportApiRunnerFactory _importApiRunnerFactory;
        private readonly IAPILog _logger;

        public CustomProviderTask(
            IKeplerServiceFactory serviceFactory,
            IIntegrationPointService integrationPointService,
            ISourceProviderService sourceProviderService,
            IIdFilesBuilder idFilesBuilder,
            ILoadFileBuilder loadFileBuilder,
            IRelativityStorageService relativityStorageService,
            ISerializer serializer,
            IJobService jobService,
            IImportApiRunnerFactory importApiRunnerFactory,
            IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _integrationPointService = integrationPointService;
            _sourceProviderService = sourceProviderService;
            _idFilesBuilder = idFilesBuilder;
            _relativityStorageService = relativityStorageService;
            _serializer = serializer;
            _jobService = jobService;
            _importApiRunnerFactory = importApiRunnerFactory;
            _logger = logger;
            _loadFileBuilder = loadFileBuilder;
        }

        public void Execute(Job job)
        {
            ExecuteAsync(job).GetAwaiter().GetResult();
        }

        private async Task ExecuteAsync(Job job)
        {
            CustomProviderJobDetails jobDetails = null;
            IntegrationPointDto integrationPointDto = null;
            ImportSettings destinationConfiguration = null;

            IStorageAccess<string> storage = null;
            DirectoryInfo importDirectory = null;

            try
            {
                jobDetails = GetJobDetails(job.JobDetails);
                integrationPointDto = _integrationPointService.Read(job.RelatedObjectArtifactID);
                destinationConfiguration = _serializer.Deserialize<ImportSettings>(integrationPointDto.DestinationConfiguration);
                storage = await _relativityStorageService.GetStorageAccessAsync().ConfigureAwait(false);

                string workspaceDirectoryPath = await _relativityStorageService.GetWorkspaceDirectoryPathAsync(job.WorkspaceID).ConfigureAwait(false);
                importDirectory = new DirectoryInfo(Path.Combine(workspaceDirectoryPath, "RIP", "Import", jobDetails.ImportJobID.ToString()));
                await storage.CreateDirectoryAsync(importDirectory.FullName).ConfigureAwait(false);

                IDataSourceProvider provider = await _sourceProviderService.GetSourceProviderAsync(job.WorkspaceID, integrationPointDto.SourceProvider);

                if (!jobDetails.Batches.Any())
                {
                    jobDetails = await CreateBatchesAsync(jobDetails.ImportJobID, job, provider, integrationPointDto, importDirectory.FullName).ConfigureAwait(false);
                }

                IImportApiRunner importApiRunner = _importApiRunnerFactory.BuildRunner(destinationConfiguration);

                List<IndexedFieldMap> fieldMapping = IndexFieldMappings(integrationPointDto.FieldMappings);

                var importJobContext = new ImportJobContext(jobDetails.ImportJobID, job.JobId, job.WorkspaceID);
                await importApiRunner.RunImportJobAsync(importJobContext, destinationConfiguration, fieldMapping);

                foreach (CustomProviderBatch batch in jobDetails.Batches)
                {
                    if (batch.IsAddedToImportQueue)
                    {
                        continue;
                    }

                    DataSourceSettings dataSourceSettings = await _loadFileBuilder.CreateDataFileAsync(
                        batch,
                        provider,
                        new IntegrationPointInfo()
                        {
                            SourceConfiguration = integrationPointDto.SourceConfiguration,
                            SecuredConfiguration = integrationPointDto.SecuredConfiguration,
                            FieldMap = fieldMapping
                        },
                        importDirectory.FullName)
                        .ConfigureAwait(false);

                    using (IImportSourceController importSourceController = await _serviceFactory.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
                    {
                        Response response = await importSourceController.AddSourceAsync(destinationConfiguration.CaseArtifactId, jobDetails.ImportJobID, batch.BatchGuid, dataSourceSettings).ConfigureAwait(false);
                        response.Validate();
                    }

                    batch.IsAddedToImportQueue = true;
                    UpdateJobDetails(job, jobDetails);
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
                if (storage != null && importDirectory != null)
                {
                    await storage.DeleteDirectoryAsync(importDirectory.FullName, new DeleteDirectoryOptions()
                    {
                        Recursive = true
                    }).ConfigureAwait(false);
                }
            }
        }

        private CustomProviderJobDetails GetJobDetails(string details)
        {
            CustomProviderJobDetails jobDetails = null;
            try
            {
                jobDetails = _serializer.Deserialize<CustomProviderJobDetails>(details);
            }
            catch (RipSerializationException ex)
            {
                _logger.LogWarning(ex, $"Unexpected content inside job-details: {details}", ex.Value);
            }

            if (jobDetails == null || jobDetails.ImportJobID == Guid.Empty)
            {
                jobDetails = new CustomProviderJobDetails()
                {
                    ImportJobID = Guid.NewGuid()
                };
            }

            _logger.LogInformation("Running custom provider job ID: {importJobId}", jobDetails.ImportJobID);

            return jobDetails;
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

            UpdateJobDetails(job, jobDetails);

            return jobDetails;
        }

        private void UpdateJobDetails(Job job, CustomProviderJobDetails jobDetails)
        {
            job.JobDetails = _serializer.Serialize(jobDetails);
            _jobService.UpdateJobDetails(job);
        }

        private static List<IndexedFieldMap> IndexFieldMappings(List<FieldMap> fieldMappings)
        {
            return fieldMappings.Select((map, i) => new IndexedFieldMap(map, i)).ToList();
        }
    }
}
