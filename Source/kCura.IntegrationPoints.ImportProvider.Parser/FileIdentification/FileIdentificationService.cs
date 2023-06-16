using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Services;
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
        private readonly IDataTransferLocationService _dataTransferLocationService;
        private readonly IFileMetadataCollector _fileMetadataCollector;
        private readonly IAPILog _logger;

        private string _workspaceFileShareDirectory;

        public FileIdentificationService(
            IOutsideInService outsideInService,
            IExporterFactory exporterFactory,
            IFileMetadataCollector fileMetadataCollector,
            IRelativityStorageService relativityStorageService,
            IDataTransferLocationService dataTransferLocationService,
            IAPILog logger)
        {
            _outsideInService = outsideInService;
            _exporterFactory = exporterFactory;
            _fileMetadataCollector = fileMetadataCollector;
            _relativityStorageService = relativityStorageService;
            _dataTransferLocationService = dataTransferLocationService;
            _logger = logger;
        }

        public void IdentifyFilesAsync(ImportProviderSettings settings, BlockingCollection<string> files)
        {
            _logger.LogInformation("Running Files Identification...");

            _workspaceFileShareDirectory = _dataTransferLocationService.GetWorkspaceFileLocationRootPath(settings.WorkspaceId);

            _logger.LogInformation("Workspace Fileshare - {fileshare}", _workspaceFileShareDirectory);

            _logger.LogInformation("Starting new Task...");
            Task.Factory.StartNew(() => StartWorkerTask(1, files.GetConsumingEnumerable()));
        }

        private async Task StartWorkerTask(int workerId, IEnumerable<string> files)
        {
            _logger.LogInformation("Field Identification Worker {workerId} started", workerId);

            // TODO ensure we can safely reuse the exporter instance (Relativity.Import creates the new one for every single request)
            using (Exporter exporter = _exporterFactory.CreateExporter())
            {
                foreach (var file in files)
                {
                    try
                    {
                        string filePath = Path.Combine(_workspaceFileShareDirectory, file);

                        string fullPath = Path.GetFullPath(filePath);

                        _logger.LogInformation("Reading file {file} - Full Path - {fullPath}", file, fullPath);

                        using (Stream stream = await OpenFileStreamAsync(fullPath))
                        {
                            FileMetadata fileMetadata = IdentifyFileMetadata(stream, exporter);

                            _logger.LogInformation("Identified FileMetadata - {@metadata}", fileMetadata);
                            if (_fileMetadataCollector.StoreMetadata(filePath, fileMetadata) == false)
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
            _logger.LogInformation("Opening Stream - {filePath}", filePath);

            var openFileParameters = new OpenFileParameters
            {
                Path = filePath,
                OpenBehavior = OpenBehavior.OpenExisting,
                ReadWriteMode = ReadWriteMode.ReadOnly,
                OpenFileOptions = new OpenFileOptions { SeekMode = SeekMode.RandomAccess }
            };

            return await _relativityStorageService.OpenFileAsync(openFileParameters, CancellationToken.None);
        }

        private FileMetadata IdentifyFileMetadata(Stream stream, Exporter exporter)
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
                // TODO we should probably report item-level-error here instead of throwing this exception
                throw;
            }
        }
    }
}
