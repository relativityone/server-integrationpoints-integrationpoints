using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1.Builders.DataSource;
using Relativity.Import.V1.Models.Sources;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class LoadFileGenerator : ILoadFileGenerator
    {
        private readonly IBatchDataSourcePreparationConfiguration _configuration;
        private readonly IDestinationServiceFactoryForUser _serviceFactory;
        private readonly ISourceWorkspaceDataReaderFactory _dataReaderFactory;
        private readonly IAPILog _logger;

        public LoadFileGenerator(
            IBatchDataSourcePreparationConfiguration configuration,
            IDestinationServiceFactoryForUser serviceFactory,
            ISourceWorkspaceDataReaderFactory dataReaderFactory,
            IAPILog logger)
        {
            _configuration = configuration;
            _serviceFactory = serviceFactory;
            _dataReaderFactory = dataReaderFactory;
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
                string value = reader[i].ToString();
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
                using (IWorkspaceManager workspaceManager = await _serviceFactory.CreateProxyAsync<IWorkspaceManager>().ConfigureAwait(false))
                {
                    WorkspaceRef workspace = new WorkspaceRef() { ArtifactID = _configuration.DestinationWorkspaceArtifactId };
                    FileShareResourceServer server = await workspaceManager.GetDefaultWorkspaceFileShareResourceServerAsync(workspace).ConfigureAwait(false);

                    string rootDirectory = Path.Combine(server.UNCPath, $"EDDS{_configuration.DestinationWorkspaceArtifactId}");
                    if (!Directory.Exists(rootDirectory))
                    {
                        throw new DirectoryNotFoundException($"Unable to create load file path. Directory: {rootDirectory} does not exist!");
                    }

                    string batchPartialDirectory = $@"Sync\{batch.ExportRunId}\{batch.BatchGuid}";
                    string fullDirectory = Path.Combine(rootDirectory, batchPartialDirectory);
                    if (!Directory.Exists(fullDirectory))
                    {
                        Directory.CreateDirectory(fullDirectory);
                    }

                    string fileName = $"{batch.BatchGuid}.dat";
                    batchFullPath = Path.Combine(fullDirectory, fileName);
                }
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
