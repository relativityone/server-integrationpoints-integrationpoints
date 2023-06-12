using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Storage;
using kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification.OutsideInServices;
using OutsideIn;
using Relativity.API;
using Relativity.Storage;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification
{
    public class FileIdentificationService : IFileIdentificationService
    {
        private const int NumberOfWorkers = 10;

        private readonly IOutsideInService _outsideInService;
        private readonly IExporterFactory _exporterFactory;
        private readonly IRelativityStorageService _relativityStorageService;
        private readonly IFileMetadataCollector _fileMetadataCollector;
        private readonly IAPILog _logger;

        public FileIdentificationService(
            IOutsideInService outsideInService,
            IExporterFactory exporterFactory,
            IFileMetadataCollector fileMetadataCollector,
            IRelativityStorageService relativityStorageService,
            IAPILog logger)
        {
            _outsideInService = outsideInService;
            _exporterFactory = exporterFactory;
            _fileMetadataCollector = fileMetadataCollector;
            _relativityStorageService = relativityStorageService;
            _logger = logger;
        }

        public async Task IdentifyFilesAsync(BlockingCollection<ImportFileInfo> files)
        {
            List<Task> workerTasks = new List<Task>();

            foreach (int workerId in Enumerable.Range(1, NumberOfWorkers))
            {
                Task task = StartWorkerTask(workerId, files.GetConsumingEnumerable());
                workerTasks.Add(task);
            }

            await Task.WhenAll(workerTasks);
        }

        private async Task StartWorkerTask(int workerId, IEnumerable<ImportFileInfo> files)
        {
            await Task.Yield();

            _logger.LogInformation("Field Identification Worker {workerId} started", workerId);

            // TODO ensure we can safely reuse the exporter instance (Relativity.Import creates the new one for every single request)
            using (Exporter exporter = _exporterFactory.CreateExporter())
            {
                foreach (var importFile in files)
                {
                    using (Stream stream = await OpenFileStreamAsync(importFile.FilePath))
                    {
                        ReadAndStoreFileMetadata(importFile.FilePath, stream, exporter);
                    }
                }
            }
        }

        private async Task<Stream> OpenFileStreamAsync(string filePath)
        {
            var openFileParameters = new OpenFileParameters
            {
                Path = filePath,
                OpenBehavior = OpenBehavior.OpenExisting,
                ReadWriteMode = ReadWriteMode.ReadOnly,
            };

            return await _relativityStorageService.OpenFileAsync(openFileParameters, CancellationToken.None);
        }

        private void ReadAndStoreFileMetadata(string filePath, Stream stream, Exporter exporter)
        {
            try
            {
                // TODO are we sure it is a proper way to get the size?
                // TODO shouldn't we call IStorageAccess.GetFileMetadataAsync() here??
                long fileSize = stream.Length;

                // TODO why do we need to reset the position?
                stream.Position = 0;
                FileFormat fileFormat = _outsideInService.IdentifyFile(stream, exporter);

                var fileMetadata = new FileMetadata(fileFormat.GetId(), fileFormat.GetDescription(), fileSize);
                if (_fileMetadataCollector.StoreMetadata(filePath, fileMetadata) == false)
                {
                    _logger.LogWarning("Unable to store file metadata - already exists {filePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file properties {filePath}", filePath);

                // TODO we should probably report item-level-error here instead of throwing this exception
                throw;
            }
        }
    }
}
