using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1.Builders.DataSource;
using Relativity.Import.V1.Models.Sources;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class LoadFileGenerator : ILoadFileGenerator
    {
        private const int _BATCH_ITEM_ERRORS_MAX_COUNT_FOR_RDO_CREATE = 1000;

        private readonly IBatchDataSourcePreparationConfiguration _configuration;
        private readonly ISourceWorkspaceDataReaderFactory _dataReaderFactory;
        private readonly IFileShareService _fileShareService;
        private readonly IItemLevelErrorLogAggregator _itemLevelErrorLogAggregator;
        private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
        private readonly IAPILog _logger;
        private readonly ConcurrentQueue<CreateJobHistoryErrorDto> _batchItemErrors;

        private IItemStatusMonitor _statusMonitor;

        public LoadFileGenerator(
            IBatchDataSourcePreparationConfiguration configuration,
            ISourceWorkspaceDataReaderFactory dataReaderFactory,
            IFileShareService fileShareService,
            IJobHistoryErrorRepository jobHistoryErrorRepository,
            IAPILog logger)
        {
            _configuration = configuration;
            _dataReaderFactory = dataReaderFactory;
            _fileShareService = fileShareService;
            _itemLevelErrorLogAggregator = new ItemLevelErrorLogAggregator(logger);
            _jobHistoryErrorRepository = jobHistoryErrorRepository;
            _logger = logger;
            _batchItemErrors = new ConcurrentQueue<CreateJobHistoryErrorDto>();
        }

        public async Task<ILoadFile> GenerateAsync(IBatch batch)
        {
            string batchPath = await CreateBatchFullPath(batch).ConfigureAwait(false);
            DataSourceSettings settings = CreateSettings(batchPath);
            await GenerateLoadFileAsync(batch, batchPath, settings).ConfigureAwait(false);
            return new LoadFile(batch.BatchGuid, batchPath, settings);
        }

        private async Task GenerateLoadFileAsync(IBatch batch, string batchPath, DataSourceSettings settings)
        {
            int readerLineNumber = 0;
            try
            {
                using (ISourceWorkspaceDataReader reader = _dataReaderFactory.CreateNativeSourceWorkspaceDataReader(batch, CancellationToken.None))
                using (StreamWriter writer = new StreamWriter(batchPath))
                {
                    _statusMonitor = reader.ItemStatusMonitor;
                    reader.OnItemReadError += HandleDataSourceItemLevelError;

                    while (reader.Read())
                    {
                        readerLineNumber++;
                        string line = GetLineContent(reader, settings);
                        writer.WriteLine(line);
                    }
                }

                await HandleDataSourceProcessingFinished(batch).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load file generator error occurred in line: {readerLineNumber}", readerLineNumber);
                await HandleDataSourceProcessingFinished(batch).ConfigureAwait(false);
                throw;
            }
        }

        private void HandleDataSourceItemLevelError(long completedItem, ItemLevelError itemLevelError)
        {
            int itemArtifactId = _statusMonitor.GetArtifactId(itemLevelError.Identifier);
            _itemLevelErrorLogAggregator.AddItemLevelError(itemLevelError, itemArtifactId);
            _statusMonitor.MarkItemAsFailed(itemLevelError.Identifier);

            CreateJobHistoryErrorDto itemError = new CreateJobHistoryErrorDto(ErrorType.Item)
            {
                ErrorMessage = itemLevelError.Message,
                SourceUniqueId = itemLevelError.Identifier
            };

            _batchItemErrors.Enqueue(itemError);

            if (_batchItemErrors.Count >= _BATCH_ITEM_ERRORS_MAX_COUNT_FOR_RDO_CREATE)
            {
                CreateJobHistoryErrors().GetAwaiter().GetResult();
            }
        }

        private async Task CreateJobHistoryErrors()
        {
            List<CreateJobHistoryErrorDto> itemLevelErrors = new List<CreateJobHistoryErrorDto>(_batchItemErrors.Count);
            while (_batchItemErrors.TryDequeue(out CreateJobHistoryErrorDto dto))
            {
                itemLevelErrors.Add(dto);
            }

            if (itemLevelErrors.Any())
            {
                await _jobHistoryErrorRepository.MassCreateAsync(_configuration.SourceWorkspaceArtifactId, _configuration.JobHistoryId, itemLevelErrors).ConfigureAwait(false);
            }
        }

        private async Task HandleDataSourceProcessingFinished(IBatch batch)
        {
            if (_batchItemErrors.Any())
            {
                await CreateJobHistoryErrors().ConfigureAwait(false);
            }

            await batch.SetFailedDocumentsCountAsync(_statusMonitor.FailedItemsCount).ConfigureAwait(false);
            await _itemLevelErrorLogAggregator.LogAllItemLevelErrorsAsync().ConfigureAwait(false);
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

            return batchFullPath;
        }
    }
}
