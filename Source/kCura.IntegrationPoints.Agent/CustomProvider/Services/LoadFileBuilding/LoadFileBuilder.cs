﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.EntityServices;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Storage;
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
        private const int _SOURCE_PROVIDER_GET_DATA_BATCH_SIZE = 1000;

        private readonly IRelativityStorageService _relativityStorageService;
        private readonly IEntityFullNameService _entityFullNameService;
        private readonly IAPILog _logger;

        public LoadFileBuilder(IRelativityStorageService relativityStorageService, IEntityFullNameService entityFullNameService, IAPILog logger)
        {
            _relativityStorageService = relativityStorageService;
            _entityFullNameService = entityFullNameService;
            _logger = logger;
        }

        public async Task<DataSourceSettings> CreateDataFileAsync(CustomProviderBatch batch, IDataSourceProvider provider, IntegrationPointInfo integrationPointInfo, string importDirectory)
        {
            try
            {
                _logger.LogInformation("Creating data file for batch index: {batchIndex}, GUID: {batchGuid}", batch.BatchID, batch.BatchGuid);

                Dictionary<string, IndexedFieldMap> destinationFieldNameToFieldMapDictionary = integrationPointInfo
                    .FieldMap
                    .OrderBy(x => x.ColumnIndex)
                    .ToDictionary(x => x.DestinationFieldName);

                IEnumerable<FieldEntry> fields = integrationPointInfo
                    .FieldMap
                    .Where(x => x.FieldMapType == FieldMapType.Normal)
                    .Select(x => x.FieldMap.SourceField);

                DataSourceProviderConfiguration providerConfig = new DataSourceProviderConfiguration(integrationPointInfo.SourceConfiguration, integrationPointInfo.SecuredConfiguration);
                IList<string> entryIds = await _relativityStorageService.ReadAllLinesAsync(batch.IDsFilePath).ConfigureAwait(false);

                _logger.LogInformation("Fields to retrieve from sourceprovider: {@fields}", fields);

                using (StorageStream dataFileStream = await GetDataFileStreamAsync(importDirectory, batch).ConfigureAwait(false))
                using (TextWriter dataFileWriter = new StreamWriter(dataFileStream))
                {
                    DataSourceSettings settings = CreateSettings(dataFileStream.StoragePath);

                    foreach (List<string> entryIdsChunk in entryIds.SplitList(_SOURCE_PROVIDER_GET_DATA_BATCH_SIZE))
                    {
                        using (IDataReader sourceProviderDataReader = provider.GetData(fields, entryIdsChunk, providerConfig))
                        {
                            await WriteFileAsync(sourceProviderDataReader, destinationFieldNameToFieldMapDictionary, settings, dataFileWriter).ConfigureAwait(false);
                        }
                    }

                    _logger.LogInformation("Successfully created data file for batch index: {batchIndex} GUID: {batchGuid} path: {path}", batch.BatchID, batch.BatchGuid, dataFileStream.StoragePath);
                    return settings;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create data file for batch index: {batchIndex}, GUID: {batchGuid}", batch.BatchID, batch.BatchGuid);
                throw;
            }
        }

        private async Task WriteFileAsync(
            IDataReader sourceProviderDataReader,
            Dictionary<string, IndexedFieldMap> destinationFieldNameToFieldMapDictionary,
            DataSourceSettings settings,
            TextWriter dataFileWriter)
        {
            while (sourceProviderDataReader.Read())
            {
                List<string> rowValues = new List<string>();

                foreach (IndexedFieldMap field in destinationFieldNameToFieldMapDictionary.Values)
                {
                    switch (field.FieldMapType)
                    {
                        case FieldMapType.Normal:
                            try
                            {
                                string value = sourceProviderDataReader[field.FieldMap.SourceField.FieldIdentifier]?.ToString() ?? string.Empty;
                                rowValues.Add(value);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to retrieve field value from Source Provider: {fieldName}", field.FieldMap.SourceField.FieldIdentifier);
                                throw;
                            }
                            break;
                        case FieldMapType.EntityFullName:
                            string fullName = _entityFullNameService.FormatFullName(destinationFieldNameToFieldMapDictionary, sourceProviderDataReader);
                            rowValues.Add(fullName);
                            break;
                    }
                }

                string line = FormatLine(settings, rowValues);
                _logger.LogInformation("Writing load file line with {numberOfValues} values: {line}", rowValues.Count, line);
                await dataFileWriter.WriteLineAsync(line).ConfigureAwait(false);
            }
        }

        private static string FormatLine(DataSourceSettings settings, List<string> rowValues)
        {
            return string.Join(settings.ColumnDelimiter.ToString(), rowValues);
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

        private async Task<StorageStream> GetDataFileStreamAsync(string directoryPath, CustomProviderBatch batch)
        {
            PrepareBatchDataFilePath(directoryPath, batch);

            try
            {
                StorageStream fileStream = await _relativityStorageService.CreateFileOrTruncateExistingAsync(batch.DataFilePath).ConfigureAwait(false);
                return fileStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open file stream: {path}", batch.DataFilePath);
                throw;
            }
        }

        private void PrepareBatchDataFilePath(string directoryPath, CustomProviderBatch batch)
        {
            string batchDataFileName = $"{batch.BatchID.ToString().PadLeft(7, '0')}.data";
            string batchDataFilePath = Path.Combine(directoryPath, batchDataFileName);
            batch.DataFilePath = batchDataFilePath;
        }
    }
}
