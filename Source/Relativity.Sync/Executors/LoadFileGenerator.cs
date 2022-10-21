﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
        private readonly IAPILog _logger;

        public LoadFileGenerator(
            IBatchDataSourcePreparationConfiguration configuration,
            ISourceWorkspaceDataReaderFactory dataReaderFactory,
            IFileShareService fileShareService,
            IAPILog logger)
        {
            _configuration = configuration;
            _dataReaderFactory = dataReaderFactory;
            _fileShareService = fileShareService;
            _logger = logger;
        }

        public async Task<ILoadFile> GenerateAsync(IBatch batch)
        {
            string batchPath = await CreateBatchFullPath(batch).ConfigureAwait(false);
            DataSourceSettings settings = CreateSettings(batchPath);
            GenerateLoadFile(batch, batchPath, settings);
            return new LoadFile(batch.BatchGuid, batchPath, settings);
        }

        private void GenerateLoadFile(IBatch batch, string batchPath, DataSourceSettings settings)
        {
            int readerLineNumber = 0;
            try
            {
                using (ISourceWorkspaceDataReader reader = _dataReaderFactory.CreateNativeSourceWorkspaceDataReader(batch, CancellationToken.None))
                using (StreamWriter writer = new StreamWriter(batchPath))
                {
                    while (reader.Read())
                    {
                        readerLineNumber++;
                        string line = GetLineContent(reader, settings);
                        writer.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load file generator error occurred in line: {readerLineNumber}", readerLineNumber);
                throw;
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
