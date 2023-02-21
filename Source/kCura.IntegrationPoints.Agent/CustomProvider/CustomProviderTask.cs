using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.Interfaces;
using Relativity;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.CustomProvider
{
    internal class CustomProviderTask : ICustomProviderTask
    {
        private readonly IIntegrationPointService _integrationPointService;
        private readonly ISourceProviderService _sourceProviderService;
        private readonly IIdFilesBuilder _idFilesBuilder;
        private readonly IRelativityStorageService _relativityStorageService;
        private readonly ISerializer _serializer;
        private readonly IJobService _jobService;
        private readonly IImportApiRunnerFactory _importApiRunnerFactory;
        private readonly IAPILog _logger;

        public CustomProviderTask(
            IIntegrationPointService integrationPointService,
            ISourceProviderService sourceProviderService,
            IIdFilesBuilder idFilesBuilder,
            IRelativityStorageService relativityStorageService,
            ISerializer serializer,
            IJobService jobService,
            IImportApiRunnerFactory importApiRunnerFactory,
            IAPILog logger)
        {
            _integrationPointService = integrationPointService;
            _sourceProviderService = sourceProviderService;
            _idFilesBuilder = idFilesBuilder;
            _relativityStorageService = relativityStorageService;
            _serializer = serializer;
            _jobService = jobService;
            _importApiRunnerFactory = importApiRunnerFactory;
            _logger = logger;
        }

        public void Execute(Job job)
        {
            ExecuteAsync(job).GetAwaiter().GetResult();
        }

        private async Task ExecuteAsync(Job job)
        {
            Guid jobId = Guid.NewGuid();

            IStorageAccess<string> storage = null;
            DirectoryInfo importDirectory = null;

            try
            {
                storage = await _relativityStorageService.GetStorageAccessAsync().ConfigureAwait(false);

                string workspaceDirectoryPath = await _relativityStorageService.GetWorkspaceDirectoryPathAsync(job.WorkspaceID).ConfigureAwait(false);
                importDirectory = new DirectoryInfo(Path.Combine(workspaceDirectoryPath, "RIP", "Import", jobId.ToString()));
                await storage.CreateDirectoryAsync(importDirectory.FullName).ConfigureAwait(false);

                IntegrationPointDto integrationPointDto = _integrationPointService.Read(job.RelatedObjectArtifactID);

                IDataSourceProvider provider = await _sourceProviderService.GetSourceProviderAsync(job.WorkspaceID, integrationPointDto.SourceProvider);

                List<CustomProviderBatch> batches = await _idFilesBuilder.BuildIdFilesAsync(provider, integrationPointDto, importDirectory.FullName).ConfigureAwait(false);

                CustomProviderJobDetails jobDetails = new CustomProviderJobDetails()
                {
                    JobID = jobId,
                    Batches = batches
                };

                job.JobDetails = _serializer.Serialize(jobDetails);
                _jobService.UpdateJobDetails(job);

                ImportApiFlowEnum importApiFlowEnum = GetImportApiFlow(integrationPointDto.DestinationConfiguration);
                IImportApiRunner importApiRunner = _importApiRunnerFactory.BuildRunner(importApiFlowEnum);
                var importJobContext = new ImportJobContext(jobDetails.JobID, job.JobId, job.WorkspaceID);

                await importApiRunner.RunImportJobAsync(importJobContext, integrationPointDto.DestinationConfiguration, WrapFieldMappings(integrationPointDto.FieldMappings));
            }
            catch (ImportApiResponseException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to run import job: {importJobId}. Error code: {errorCode}. Error message: {errorMessage}",
                    ex.Response.ImportJobID,
                    ex.Response.ErrorCode,
                    ex.Response.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute Custom Provider job");

                // TODO REL-806942 Mark job as failed
                // There is a newly created method IntegrationPointService.MarkJobAsFailed() (currently private)
                // We can extract this method to the separate service and extend it with other JobHistory(Error) use cases
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

        private static ImportApiFlowEnum GetImportApiFlow(string destinationConfiguration)
        {
            ImportSettings settings = _serializer.Deserialize<ImportSettings>(destinationConfiguration);
            return settings.ArtifactTypeId == (int)ArtifactType.Document
                ? ImportApiFlowEnum.Document
                : ImportApiFlowEnum.Rdo;
        }

        private static List<FieldMapWrapper> WrapFieldMappings(List<FieldMap> fieldMappings)
        {
            return fieldMappings.Select((map, i) => new FieldMapWrapper(map, i)).ToList();
        }
    }
}
