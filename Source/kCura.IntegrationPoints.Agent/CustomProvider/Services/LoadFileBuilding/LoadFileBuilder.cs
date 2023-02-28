using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare;
using kCura.IntegrationPoints.Core.Models;
using Relativity.API;
using Relativity.Import.V1.Builders.DataSource;
using Relativity.Import.V1.Models.Sources;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.LoadFileBuilding
{
    internal class LoadFileBuilder : ILoadFileBuilder
    {
        private readonly IRelativityStorageService _relativityStorageService;
        private readonly IAPILog _logger;

        public LoadFileBuilder(IRelativityStorageService relativityStorageService, IAPILog logger)
        {
            _relativityStorageService = relativityStorageService;
            _logger = logger;
        }

        public async Task<DataSourceSettings> CreateDataFileAsync(IStorageAccess<string> storage, CustomProviderBatch batch, IDataSourceProvider provider, IntegrationPointDto integrationPointDto, string importDirectory, List<FieldMapWrapper> fieldMap)
        {
            try
            {
                _logger.LogInformation("Creating data file for batch index: {batchIndex}", batch.BatchID);

                List<FieldMapWrapper> orderedFieldMap = fieldMap.OrderBy(x => x.ColumnIndex).ToList();

                IEnumerable<FieldEntry> fields = integrationPointDto.FieldMappings.Select(x => x.SourceField);
                DataSourceProviderConfiguration providerConfig = new DataSourceProviderConfiguration(integrationPointDto.SourceConfiguration, integrationPointDto.SecuredConfiguration);
                IList<string> entryIds = await ReadLinesAsync(storage, batch.IDsFilePath).ConfigureAwait(false);

                using (IDataReader sourceProviderDataReader = provider.GetData(fields, entryIds, providerConfig))
                using (StorageStream dataFileStream = await GetDataFileStreamAsync(importDirectory, batch.BatchID).ConfigureAwait(false))
                using (TextWriter dataFileWriter = new StreamWriter(dataFileStream))
                {
                    DataSourceSettings settings = CreateSettings(dataFileStream.StoragePath);

                    while (sourceProviderDataReader.Read())
                    {
                        List<string> rowValues = new List<string>();

                        foreach (FieldMapWrapper field in orderedFieldMap)
                        {
                            string value = sourceProviderDataReader[field.FieldMap.SourceField.ActualName]?.ToString() ?? string.Empty;
                            rowValues.Add(value);
                        }

                        string line = string.Join(settings.ColumnDelimiter.ToString(), rowValues);
                        await dataFileWriter.WriteLineAsync(line).ConfigureAwait(false);
                    }

                    _logger.LogInformation("Successfully created data file for batch index: {batchIndex} path: {path}", batch.BatchID, dataFileStream.StoragePath);

                    return settings;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create data file for batch index: {batchIndex}", batch.BatchID);
                throw;
            }
        }

        private DataSourceSettings CreateSettings(string batchPath)
        {
            return DataSourceSettingsBuilder.Create()
                .ForLoadFile(batchPath)
                .WithDelimiters(d => d
                    .WithColumnDelimiters(LoadFileOptions._DEFAULT_COLUMN_DELIMITER_ASCII)
                    .WithQuoteDelimiter(LoadFileOptions._DEFAULT_QUOTE_DELIMITER_ASCII)
                    .WithNewLineDelimiter(LoadFileOptions._DEFAULT_NEWLINE_DELIMITER_ASCII)
                    .WithNestedValueDelimiter(LoadFileOptions._DEFAULT_NESTED_VALUE_ASCII)
                    .WithMultiValueDelimiter(LoadFileOptions._DEFAULT_MULTI_VALUE_ASCII))
                .WithoutFirstLineContainingHeaders()
                .WithEndOfLineForWindows()
                .WithStartFromBeginning()
                .WithDefaultEncoding()
                .WithDefaultCultureInfo();
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
    }
}
