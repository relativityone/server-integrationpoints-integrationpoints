using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.InstanceSettings;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Storage;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.IdFileBuilding
{
    public class IdFilesBuilder : IIdFilesBuilder
    {
        private readonly IInstanceSettings _instanceSettings;
        private readonly IRelativityStorageService _storageService;
        private readonly IAPILog _logger;

        public IdFilesBuilder(IInstanceSettings instanceSettings, IRelativityStorageService storageService, IAPILog logger)
        {
            _instanceSettings = instanceSettings;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<List<CustomProviderBatch>> BuildIdFilesAsync(IDataSourceProvider provider, IntegrationPointDto integrationPoint, string directoryPath)
        {
            IDataReader reader = await GetIdsReaderAsync(provider, integrationPoint).ConfigureAwait(false);
            int batchSize = await _instanceSettings.GetCustomProviderBatchSizeAsync().ConfigureAwait(false);

            _logger.LogInformation("Writing files with IDs using batch size: {batchSize}", batchSize);

            List<CustomProviderBatch> batches = new List<CustomProviderBatch>();
            int batchIndex = 0;
            int numberOfRecordsInBatch = 0;

            bool read = reader.Read();

            while (read)
            {
                using (StorageStream fileStream = await GetFileStreamAsync(directoryPath, batchIndex).ConfigureAwait(false))
                using (TextWriter textWriter = new StreamWriter(fileStream))
                {
                    string idFilePath = fileStream.StoragePath;

                    _logger.LogInformation("Writing ID file for batch index: {batchIndex}, file path: {path}", batchIndex, idFilePath);

                    while (read && numberOfRecordsInBatch < batchSize)
                    {
                        string recordId = reader.GetString(0);
                        if (string.IsNullOrEmpty(recordId))
                        {
                            throw new InvalidDataException($"ID value is null for record: {numberOfRecordsInBatch}");
                        }
                        await textWriter.WriteLineAsync(recordId).ConfigureAwait(false);
                        numberOfRecordsInBatch++;

                        read = reader.Read();
                    }

                    CustomProviderBatch batch = new CustomProviderBatch()
                    {
                        BatchGuid = Guid.NewGuid(),
                        BatchID = batchIndex,
                        IDsFilePath = idFilePath,
                        NumberOfRecords = numberOfRecordsInBatch
                    };

                    _logger.LogInformation("Finished writing ID file for batch index {batch}, file path: {path}, batch GUID: {guid}", batchIndex, idFilePath, batch.BatchGuid);
                    batches.Add(batch);

                    batchIndex++;
                    numberOfRecordsInBatch = 0;
                }
            }

            return batches;
        }

        private async Task<StorageStream> GetFileStreamAsync(string directoryPath, int batchIndex)
        {
            string batchIDsFileName = $"{batchIndex.ToString().PadLeft(7, '0')}.id";
            string batchIDsFilePath = Path.Combine(directoryPath, batchIDsFileName);

            try
            {
                StorageStream fileStream = await _storageService.CreateFileOrTruncateExistingAsync(batchIDsFilePath).ConfigureAwait(false);
                return fileStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open file stream: {path}", batchIDsFilePath);
                throw;
            }
        }

        private Task<IDataReader> GetIdsReaderAsync(IDataSourceProvider provider, IntegrationPointDto integrationPoint)
        {
            FieldEntry idField = integrationPoint.FieldMappings.FirstOrDefault(x => x.FieldMapType == FieldMapTypeEnum.Identifier)?.SourceField;
            try
            {
                _logger.LogInformation("Retrieving record IDs from custom provider");
                IDataReader reader = provider.GetBatchableIds(idField, new DataSourceProviderConfiguration(integrationPoint.SourceConfiguration, integrationPoint.SecuredConfiguration));
                return Task.FromResult(reader);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrive record IDs from custom provider");
                throw;
            }
        }
    }
}
