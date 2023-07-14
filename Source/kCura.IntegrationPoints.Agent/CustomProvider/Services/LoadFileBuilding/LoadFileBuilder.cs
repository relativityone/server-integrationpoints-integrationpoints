using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Storage;
using Relativity.API;
using Relativity.Import.V1.Builders.DataSource;
using Relativity.Import.V1.Models.Sources;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.Services.Exceptions;
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

        public async Task<DataSourceSettings> CreateDataFileAsync(CustomProviderBatch batch, IDataSourceProvider provider, IntegrationPointInfo integrationPointInfo, string importDirectory)
        {
            try
            {
                _logger.LogInformation("Creating data file for batch index: {batchIndex}", batch.BatchID);

                List<IndexedFieldMap> orderedFieldMap = integrationPointInfo.FieldMap.OrderBy(x => x.ColumnIndex).ToList();

                IEnumerable<FieldEntry> fields = integrationPointInfo.FieldMap.Select(x => x.FieldMap.SourceField);
                DataSourceProviderConfiguration providerConfig = new DataSourceProviderConfiguration(integrationPointInfo.SourceConfiguration, integrationPointInfo.SecuredConfiguration);
                IList<string> entryIds = await _relativityStorageService.ReadAllLinesAsync(batch.IDsFilePath).ConfigureAwait(false);

                using (IDataReader sourceProviderDataReader = provider.GetData(fields, entryIds, providerConfig))
                using (StorageStream dataFileStream = await GetDataFileStreamAsync(importDirectory, batch).ConfigureAwait(false))
                using (TextWriter dataFileWriter = new StreamWriter(dataFileStream))
                {
                    DataSourceSettings settings = CreateSettings(dataFileStream.StoragePath);
                    await WriteFileAsync(sourceProviderDataReader, orderedFieldMap, settings, dataFileWriter, integrationPointInfo.ArtifactTypeId).ConfigureAwait(false);
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

        private static async Task WriteFileAsync(
            IDataReader sourceProviderDataReader,
            List<IndexedFieldMap> orderedFieldMap,
            DataSourceSettings settings,
            TextWriter dataFileWriter,
            int artifactTypeId)
        {
            int firstNameSourceFieldId;
            int lastNameSourceFieldId;

            try
            {
                firstNameSourceFieldId = orderedFieldMap.Single(x => x.FieldMap.SourceField.DisplayName == EntityFieldNames.FirstName).ColumnIndex;
                lastNameSourceFieldId = orderedFieldMap.Single(x => x.FieldMap.SourceField.DisplayName == EntityFieldNames.LastName).ColumnIndex;

            }
            catch
            {
                throw new NotFoundException($"{EntityFieldNames.FirstName} or/and {EntityFieldNames.LastName} not found in fields mapping.");
            }

            while (sourceProviderDataReader.Read())
            {
                List<string> rowValues = new List<string>();

                foreach (IndexedFieldMap field in orderedFieldMap)
                {
                    string value = sourceProviderDataReader[field.FieldMap.SourceField.ActualName]?.ToString() ?? string.Empty;
                    rowValues.Add(value);
                }

                bool isFullNameMapped = orderedFieldMap.Select(x => x.FieldMap.DestinationField.DisplayName).Contains(EntityFieldNames.FullName);

                if (artifactTypeId == ObjectTypeIds.Entity && !isFullNameMapped)
                {
                    string firstName = sourceProviderDataReader[orderedFieldMap[firstNameSourceFieldId].FieldMap.SourceField.ActualName]?.ToString() ?? string.Empty;
                    string lastName = sourceProviderDataReader[orderedFieldMap[lastNameSourceFieldId].FieldMap.SourceField.ActualName]?.ToString() ?? string.Empty;

                    string fullName = GenerateFullName(lastName, firstName);
                    rowValues.Add(fullName);
                }

                string line = FormatLine(settings, rowValues);
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

        private static string GenerateFullName(string lastName, string firstName)
        {
            string fullName = string.Empty;
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                fullName = lastName;
            }
            if (!string.IsNullOrWhiteSpace(firstName))
            {
                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    fullName += ", ";
                }
                fullName += firstName;
            }
            return fullName;
        }
    }
}
