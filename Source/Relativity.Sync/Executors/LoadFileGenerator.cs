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
        private const int _DEFAULT_COLUMN_DELIMITER_ASCII = 020;

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

        public async Task<ILoadFile> Generate(IBatch batch)
        {
            ILoadFile loadFile = null;
            try
            {
                string batchPath = await CreateBatchFullPath(batch.BatchGuid).ConfigureAwait(false);
                DataSourceSettings settings = CreateSettings(batchPath);
                GenerateLoadFile(batch, batchPath);
                loadFile = new LoadFile(batch.BatchGuid, batchPath, settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load file creation failed for batch GUID: {guid} ArtifactId: {artifactId}", batch.BatchGuid, batch.ArtifactId);
                throw ex;
            }

            return loadFile;
        }

        private void GenerateLoadFile(IBatch batch, string batchPath)
        {
            using (ISourceWorkspaceDataReader reader = _dataReaderFactory.CreateNativeSourceWorkspaceDataReader(batch, CancellationToken.None))
            {
                using (StreamWriter writer = new StreamWriter(batchPath))
                {
                    while (reader.Read())
                    {
                        string line = GetLineContent(reader);
                        writer.WriteLine(line);
                    }
                }
            }
        }

        private string GetLineContent(ISourceWorkspaceDataReader reader)
        {
            List<string> rowValues = new List<string>();
            char delimiter = (char)_DEFAULT_COLUMN_DELIMITER_ASCII;

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string value = reader[i].ToString();
                rowValues.Add(value);
            }

            return string.Join($"{delimiter}", rowValues);

            // OPENED QUESTION: should we add "endLine" char after every line if we are using writer.WriteLine(line) instruction?
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

        private async Task<string> CreateBatchFullPath(Guid batchGuid)
        {
            string batchFullPath = string.Empty;

            using (IWorkspaceManager workspaceManager = await _serviceFactory.CreateProxyAsync<IWorkspaceManager>().ConfigureAwait(false))
            {
                WorkspaceRef workspace = new WorkspaceRef() { ArtifactID = _configuration.DestinationWorkspaceArtifactId };
                FileShareResourceServer server = await workspaceManager.GetDefaultWorkspaceFileShareResourceServerAsync(workspace).ConfigureAwait(false);
                string directoryPath = Path.Combine(server.UNCPath, $@"EDDS{_configuration.DestinationWorkspaceArtifactId}\Sync");
                string fileName = $"{batchGuid}.dat";

                batchFullPath = Path.Combine(directoryPath, fileName);
            }

            return batchFullPath;
        }
    }
}
