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

namespace Relativity.Sync.Executors
{
    internal class LoadFileGenerator : ILoadFileGenerator
    {
        private readonly IBatchDataSourcePreparationConfiguration _configuration;
        private readonly ISourceWorkspaceDataReaderFactory _dataReaderFactory;
        private readonly IFileShareService _fileShareService;
        private readonly IItemLevelErrorHandlerFactory _itemLevelErrorHandlerFactory;
        private readonly IAPILog _logger;

        public LoadFileGenerator(
            IBatchDataSourcePreparationConfiguration configuration,
            ISourceWorkspaceDataReaderFactory dataReaderFactory,
            IFileShareService fileShareService,
            IItemLevelErrorHandlerFactory itemLevelErrorHandlerFactory,
            IAPILog logger)
        {
            _configuration = configuration;
            _dataReaderFactory = dataReaderFactory;
            _fileShareService = fileShareService;
            _itemLevelErrorHandlerFactory = itemLevelErrorHandlerFactory;
            _logger = logger;
        }

        public async Task<ILoadFile> GenerateAsync(IBatch batch, CompositeCancellationToken token)
        {
            string batchPath = await CreateBatchFullPath(batch).ConfigureAwait(false);
            DataSourceSettings settings = CreateSettings(batchPath);
            await GenerateLoadFileAsync(batch, batchPath, settings, token).ConfigureAwait(false);
            return new LoadFile(batch.BatchGuid, batchPath, settings);
        }

        private async Task GenerateLoadFileAsync(IBatch batch, string batchPath, DataSourceSettings settings, CompositeCancellationToken token)
        {
            int readerLineNumber = 0;
            using (ISourceWorkspaceDataReader reader = _dataReaderFactory.CreateNativeSourceWorkspaceDataReader(batch, token.AnyReasonCancellationToken))
            {
                IItemLevelErrorHandler itemLevelErrorHandler = _itemLevelErrorHandlerFactory.Create(reader.ItemStatusMonitor);
                try
                {
                    _logger.LogInformation("Generating LoadFile for Batch {batchId}", batch.ArtifactId);
                    reader.OnItemReadError += itemLevelErrorHandler.HandleItemLevelError;
                    using (StreamWriter writer = new StreamWriter(batchPath, append: true))
                    {
                        while (reader.Read())
                        {
                            string line = GetLineContent(reader, settings);
                            writer.WriteLine(line);
                            readerLineNumber++;
                        }
                    }

                    await HandleBatchStatusOnProcessingStop(token, batch, readerLineNumber).ConfigureAwait(false);
                    await itemLevelErrorHandler.HandleDataSourceProcessingFinishedAsync(batch).ConfigureAwait(false);

                    _logger.LogInformation("LoadFile for batch {batchId} was written with {recordsCount} records - {path}", batch.ArtifactId, readerLineNumber, batchPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Load file generator error occurred in line: {readerLineNumber}", readerLineNumber);
                    await itemLevelErrorHandler.HandleDataSourceProcessingFinishedAsync(batch).ConfigureAwait(false);
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
                    .WithDefaultDelimiters()
                    .WithoutFirstLineContainingHeaders()
                    .WithEndOfLineForWindows()
                    .WithStartFromBeginning()
                    .WithDefaultEncoding()
                    .WithDefaultCultureInfo();
        }

        private async Task<string> CreateBatchFullPath(IBatch batch)
        {
            _logger.LogInformation("Preparing LoadFile path for Batch {batchId} - {batchGuid}...", batch.ArtifactId, batch.BatchGuid);
            string batchFullPath = string.Empty;
            try
            {
                int workspaceId = _configuration.DestinationWorkspaceArtifactId;

                string fileSharePath = await _fileShareService.GetWorkspaceFileShareLocationAsync(workspaceId)
                    .ConfigureAwait(false);

                if (!Directory.Exists(fileSharePath))
                {
                    throw new DirectoryNotFoundException($"Unable to create load file path. Directory: {fileSharePath} does not exist!");
                }

                string batchPartialDirectory = $@"Sync\{batch.ExportRunId}\{batch.BatchGuid}";
                string fullDirectory = Path.Combine(fileSharePath, batchPartialDirectory);
                if (!Directory.Exists(fullDirectory))
                {
                    Directory.CreateDirectory(fullDirectory);
                }

                string fileName = $"{batch.BatchGuid}.dat";
                batchFullPath = Path.Combine(fullDirectory, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not build load file path for batch {batchGuid}", batch.BatchGuid);
                throw;
            }

            _logger.LogInformation("LoadFile Path for Batch {batchId} was prepared - {batchPath}", batch.ArtifactId, batchFullPath);
            return batchFullPath;
        }

        private async Task HandleBatchStatusOnProcessingStop(CompositeCancellationToken token, IBatch batch, int startIndexForProcessingAfterResume)
        {
            if (token.IsStopRequested)
            {
                _logger.LogInformation("Cancellation requested for job ID: {jobId}, batch GUID: {batchId}", _configuration.ExportRunId, batch.BatchGuid);
                await batch.SetStatusAsync(BatchStatus.Cancelled).ConfigureAwait(false);
            }

            if (token.IsDrainStopRequested)
            {
                _logger.LogInformation("Drain stop requested for job ID: {jobId}, batch GUID: {batchId}", _configuration.ExportRunId, batch.BatchGuid);
                await batch.SetStatusAsync(BatchStatus.Paused).ConfigureAwait(false);
                await batch.SetStartingIndexAsync(startIndexForProcessingAfterResume).ConfigureAwait(false);
            }
        }
    }
}
