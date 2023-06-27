using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Storage;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification.OutsideInServices;
using OutsideIn;
using Relativity.Storage;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification
{
    public class FileIdentificationService : IFileIdentificationService
    {
        private const int NumberOfWorkers = 10;

        private readonly IOutsideInService _outsideInService;
        private readonly IRelativityStorageService _relativityStorageService;
        private readonly IFileMetadataCollector _fileMetadataCollector;

        private readonly ILogger<FileIdentificationService> _logger;
        private readonly IDiagnosticLog _diagnosticLogger;

        public FileIdentificationService(
            IOutsideInService outsideInService,
            IFileMetadataCollector fileMetadataCollector,
            IRelativityStorageService relativityStorageService,
            ILogger<FileIdentificationService> logger,
            IDiagnosticLog diagnosticLogger)
        {
            _outsideInService = outsideInService;
            _fileMetadataCollector = fileMetadataCollector;
            _relativityStorageService = relativityStorageService;
            _logger = logger;
            _diagnosticLogger = diagnosticLogger;
        }

        public async Task IdentifyFilesAsync(BlockingCollection<string> files)
        {
            _logger.LogInformation("Running Files Identification...");

            List<Task> workerTasks = new List<Task>();

            foreach (int workerId in Enumerable.Range(1, NumberOfWorkers))
            {
                _logger.LogInformation("Starting new Field Identification Worker  {workerId}", workerId);
                Task task = StartWorkerTask(workerId, files.GetConsumingEnumerable());
                workerTasks.Add(task);
            }

            await Task.WhenAll(workerTasks);
        }

        private async Task StartWorkerTask(int workerId, IEnumerable<string> files)
        {
            await Task.Yield();

            _logger.LogInformation("Field Identification Worker {workerId} started", workerId);

            foreach (var fullPath in files)
            {
                try
                {
                    _diagnosticLogger.LogDiagnostic("Identifying file {fullPath}", fullPath);

                    using (Stream stream = await OpenFileStreamAsync(fullPath).ConfigureAwait(false))
                    {
                        FileMetadata fileMetadata = IdentifyFileMetadata(stream);
                        _diagnosticLogger.LogDiagnostic("Identified FileMetadata - {@metadata}", fileMetadata);

                        if (_fileMetadataCollector.StoreMetadata(fullPath, fileMetadata) == false)
                        {
                            _logger.LogWarning("Unable to store file metadata - already exists {filePath}", fullPath);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error occurred when getting the file metadata - {filePath}", fullPath);
                }
            }
        }

        private async Task<Stream> OpenFileStreamAsync(string filePath)
        {
            _diagnosticLogger.LogDiagnostic("Opening Stream - {filePath}", filePath);

            var openFileParameters = new OpenFileParameters
            {
                Path = filePath,
                OpenBehavior = OpenBehavior.OpenExisting,
                ReadWriteMode = ReadWriteMode.ReadOnly,
                OpenFileOptions = new OpenFileOptions { SeekMode = SeekMode.RandomAccess }
            };

            return await _relativityStorageService.OpenFileAsync(openFileParameters, CancellationToken.None).ConfigureAwait(false);
        }

        private FileMetadata IdentifyFileMetadata(Stream stream)
        {
            long fileSize = stream.Length;
            FileFormat fileFormat = _outsideInService.IdentifyFile(stream);
            return new FileMetadata(fileFormat.GetId(), fileFormat.GetDescription(), fileSize);
        }
    }
}
