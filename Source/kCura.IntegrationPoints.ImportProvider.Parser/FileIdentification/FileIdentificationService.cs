using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Storage;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification.OutsideInServices;
using OutsideIn;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Storage;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification
{
    public class FileIdentificationService : IFileIdentificationService
    {
        //private const int NumberOfWorkers = 10;
        private const int NumberOfWorkers = 1;

        private readonly IOutsideInService _outsideInService;
        private readonly IExporterFactory _exporterFactory;
        private readonly IRelativityStorageService _relativityStorageService;
        private readonly IFileMetadataCollector _fileMetadataCollector;
        private readonly IAPILog _logger;

        private string _loadFileDirectory;

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

        public async Task IdentifyFilesAsync(ImportProviderSettings settings, BlockingCollection<string> files)
        {
            List<Task> workerTasks = new List<Task>();

            foreach (int workerId in Enumerable.Range(1, NumberOfWorkers))
            {
                Task task = StartWorkerTask(workerId, files.GetConsumingEnumerable());
                workerTasks.Add(task);
            }

            await Task.WhenAll(workerTasks);
        }

        private async Task StartWorkerTask(int workerId, IEnumerable<string> files)
        {
            await Task.Yield();

            _logger.LogInformation("Field Identification Worker {workerId} started", workerId);

            // TODO ensure we can safely reuse the exporter instance (Relativity.Import creates the new one for every single request)
            using (Exporter exporter = _exporterFactory.CreateExporter())
            {
                foreach (var file in files)
                {
                    try
                    {
                        using (Stream stream = await OpenFileStreamAsync(file))
                        {
                            FileMetadata fileMetadata = IdentifyFileMetadata(file, stream, exporter);
                            if (_fileMetadataCollector.StoreMetadata(file, fileMetadata) == false)
                            {
                                _logger.LogWarning("Unable to store file metadata - already exists {filePath}", file);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error ocurred when opening the File - {file}", file);
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

        private FileMetadata IdentifyFileMetadata(string filePath, Stream stream, Exporter exporter)
        {
            try
            {
                // TODO are we sure it is a proper way to get the size?
                // TODO shouldn't we call IStorageAccess.GetFileMetadataAsync() here??
                long fileSize = stream.Length;

                FileFormat fileFormat = _outsideInService.IdentifyFile(stream, exporter);

                return new FileMetadata(fileFormat.GetId(), fileFormat.GetDescription(), fileSize);
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
