using System;
using System.Collections.Generic;
using System.Data;
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
using Relativity.IntegrationPoints.Contracts.Models;
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

                CustomProviderJobDetails jobDetails = _serializer.Deserialize<CustomProviderJobDetails>(job.JobDetails);

                if (jobDetails?.Batches == null || !jobDetails.Batches.Any())
                {
                    jobDetails = await CreateBatchesAsync(jobId, job, provider, integrationPointDto, importDirectory.FullName).ConfigureAwait(false);
                }

                foreach (CustomProviderBatch batch in jobDetails.Batches)
                {
                    if (batch.IsAddedToImportQueue)
                    {
                        continue;
                    }

                    batch.DataFilePath = await CreateDataFileAsync(storage, batch, provider, integrationPointDto, importDirectory.FullName).ConfigureAwait(false);

                    // TODO add file to import

                    batch.IsAddedToImportQueue = true;
                    UpdateJobDetails(job, jobDetails);
                }

                ImportApiFlowEnum importApiFlowEnum = GetImportApiFlow(integrationPointDto.DestinationConfiguration);
                IImportApiRunner importApiRunner = _importApiRunnerFactory.BuildRunner(importApiFlowEnum);
                var importJobContext = new ImportJobContext(jobDetails.ImportJobID, job.JobId, job.WorkspaceID);
                
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

        private async Task<string> CreateDataFileAsync(IStorageAccess<string> storage, CustomProviderBatch batch, IDataSourceProvider provider, IntegrationPointDto integrationPointDto, string importDirectory)
        {
            try
            {
                _logger.LogInformation("Creating data file for batch index: {batchIndex}", batch.BatchID);

                IEnumerable<FieldEntry> fields = integrationPointDto.FieldMappings.Select(x => x.SourceField);
                DataSourceProviderConfiguration providerConfig = new DataSourceProviderConfiguration(integrationPointDto.SourceConfiguration, integrationPointDto.SecuredConfiguration);
                IList<string> entryIds = await ReadLinesAsync(storage, batch.IDsFilePath).ConfigureAwait(false);

                using (IDataReader sourceProviderDataReader = provider.GetData(fields, entryIds, providerConfig))
                using (StorageStream dataFileStream = await GetDataFileStreamAsync(importDirectory, batch.BatchID).ConfigureAwait(false))
                using (TextWriter dataFileWriter = new StreamWriter(dataFileStream))
                {
                    while (sourceProviderDataReader.Read())
                    {
                        List<string> rowValues = new List<string>();

                        for (int i = 0; i < sourceProviderDataReader.FieldCount; i++)
                        {
                            string value = sourceProviderDataReader[i]?.ToString() ?? string.Empty;
                            rowValues.Add(value);
                        }

                        string line = string.Join($",", rowValues);
                        await dataFileWriter.WriteLineAsync(line).ConfigureAwait(false);
                    }

                    _logger.LogInformation("Successfully created data file for batch index: {batchIndex} path: {path}", batch.BatchID, dataFileStream.StoragePath);

                    return dataFileStream.StoragePath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create data file for batch index: {batchIndex}", batch.BatchID);
                throw;
            }
        }

        private async Task<IList<string>> ReadLinesAsync(IStorageAccess<string> storage, string filePath)
        {
            try
            {
                _logger.LogInformation("Reading all lines from file: {path}", filePath);

                List<string> lines = new List<string>();

                using (StorageStream storageStream = await storage.OpenFileAsync(filePath, OpenBehavior.OpenExisting, ReadWriteMode.ReadOnly).ConfigureAwait(false))
                using (TextReader reader = new StreamReader(storageStream))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        lines.Add(line);
                    }
                }

                _logger.LogInformation("Successfully read {lines} lines from file: {path}", lines.Count, filePath);

                return lines;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read lines from file: {path}", filePath);
                throw;
            }
        }

        private async Task<StorageStream> GetDataFileStreamAsync(string directoryPath, int batchIndex)
        {
            string batchDataFileName = $"{batchIndex.ToString().PadLeft(7, '0')}.data";
            string batchDataFilePath = Path.Combine(directoryPath, batchDataFileName);

            try
            {
                StorageStream fileStream = await _relativityStorageService.CreateFileOrTruncateExistingAsync(batchDataFilePath).ConfigureAwait(false);
                return fileStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open file stream: {path}", batchDataFilePath);
                throw;
            }
        }

        private ImportApiFlowEnum GetImportApiFlow(string destinationConfiguration)
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
