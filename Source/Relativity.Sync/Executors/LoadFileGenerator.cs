using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1.Builders.DataSource;
using Relativity.Import.V1.Models.Sources;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Executors
{
    internal class LoadFileGenerator : ILoadFileGenerator
    {
        private readonly IBatchDataSourcePreparationConfiguration _configuration;
        private readonly ISourceWorkspaceDataReaderFactory _dataReaderFactory;
        private readonly IItemLevelErrorHandler _itemLevelErrorHandler;
        private readonly IInstanceSettings _instanceSettings;
        private readonly ILoadFilePathService _pathService;
        private readonly IAPILog _logger;

        public LoadFileGenerator(
            IBatchDataSourcePreparationConfiguration configuration,
            ISourceWorkspaceDataReaderFactory dataReaderFactory,
            IItemLevelErrorHandler itemLevelErrorHandler,
            IInstanceSettings instanceSettings,
            ILoadFilePathService pathService,
            IAPILog logger)
        {
            _configuration = configuration;
            _dataReaderFactory = dataReaderFactory;
            _itemLevelErrorHandler = itemLevelErrorHandler;
            _instanceSettings = instanceSettings;
            _pathService = pathService;
            _logger = logger;
        }

        public async Task<ILoadFile> GenerateAsync(IBatch batch, CompositeCancellationToken token)
        {
            string batchPath = await _pathService.GenerateBatchLoadFileAsync(batch).ConfigureAwait(false);
            DataSourceSettings settings = CreateSettings(batchPath);
            await GenerateLoadFileAsync(batch, batchPath, settings, token).ConfigureAwait(false);
            return new LoadFile(batch.BatchGuid, batchPath, settings);
        }

        private async Task GenerateLoadFileAsync(IBatch batch, string batchPath, DataSourceSettings settings, CompositeCancellationToken token)
        {
            using (ISourceWorkspaceDataReader reader = _dataReaderFactory.CreateNativeSourceWorkspaceDataReader(batch, token.AnyReasonCancellationToken))
            {
                try
                {
                    _logger.LogInformation("Generating LoadFile for Batch {batchId}", batch.ArtifactId);
                    reader.OnItemReadError += _itemLevelErrorHandler.HandleItemLevelError;
                    using (StreamWriter writer = new StreamWriter(batchPath, append: true))
                    {
                        int updateBatchStatusCount = await _instanceSettings.GetImportAPIBatchStatusItemsUpdateCountAsync().ConfigureAwait(false);
                        while (reader.Read())
                        {
                            string line = GetLineContent(reader, settings);
                            writer.WriteLine(line);
                            if (reader.ItemStatusMonitor.ReadItemsCount % updateBatchStatusCount == 0)
                            {
                                await HandleBatchStatusAsync(token, batch, reader.ItemStatusMonitor).ConfigureAwait(false);
                            }
                        }
                    }

                    await HandleBatchStatusAsync(token, batch, reader.ItemStatusMonitor).ConfigureAwait(false);
                    await _itemLevelErrorHandler.HandleRemainingErrorsAsync()
                        .ConfigureAwait(false);

                    _logger.LogInformation(
                        "LoadFile for batch {batchId} was written with {recordsCount} records - {path}",
                        batch.ArtifactId,
                        reader.ItemStatusMonitor.ProcessedItemsCount,
                        batchPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Load file generator error occurred in line: {readerLineNumber}", reader.ItemStatusMonitor.ReadItemsCount);
                    await _itemLevelErrorHandler.HandleRemainingErrorsAsync()
                            .ConfigureAwait(false);
                    await batch.SetStatusAsync(BatchStatus.Failed).ConfigureAwait(false);
                    throw;
                }
            }
        }

        private string GetLineContent(ISourceWorkspaceDataReader reader, DataSourceSettings settings)
        {
            List<string> rowValues = new List<string>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string value = reader[i]?.ToString() ?? string.Empty;
                rowValues.Add(value);
            }

            return string.Join($"{settings.ColumnDelimiter}", rowValues);

            // TBD: Should we add settings.NewLineDelimiter here if we are processing returned value as writer.WriteLine() parameter?
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

        private async Task HandleBatchStatusAsync(CompositeCancellationToken token, IBatch batch, IItemStatusMonitor monitor)
        {
            await batch.SetFailedReadDocumentsCount(monitor.FailedItemsCount).ConfigureAwait(false);
            await batch.SetReadDocumentsCount(monitor.ReadItemsCount).ConfigureAwait(false);

            if (token.IsStopRequested)
            {
                _logger.LogInformation("Cancellation requested for job ID: {jobId}, batch GUID: {batchId}", _configuration.ExportRunId, batch.BatchGuid);
                await batch.SetStatusAsync(BatchStatus.Cancelled).ConfigureAwait(false);
                return;
            }

            if (token.IsDrainStopRequested)
            {
                _logger.LogInformation("Drain stop requested for job ID: {jobId}, batch GUID: {batchId}", _configuration.ExportRunId, batch.BatchGuid);
                await batch.SetStatusAsync(BatchStatus.Paused).ConfigureAwait(false);
                await batch.SetStartingIndexAsync(monitor.ReadItemsCount).ConfigureAwait(false);
                return;
            }
        }
    }
}
