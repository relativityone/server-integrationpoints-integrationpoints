using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using kCura.IntegrationPoints.ImportProvider.FileIdentification.OutsideInServices;
using OutsideIn;
using Relativity.API;

namespace kCura.IntegrationPoints.ImportProvider.FileIdentification
{
    public class FileIdentificationService : IFileIdentificationService
    {
        private readonly IOutsideInService _outsideInService;
        private readonly IExporterFactory _exporterFactory;
        private readonly IAPILog _logger;

        public FileIdentificationService(IOutsideInService outsideInService, IExporterFactory exporterFactory, IAPILog logger)
        {
            _outsideInService = outsideInService;
            _exporterFactory = exporterFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<FileProperties>> IdentifyFilesAsync(IEnumerable<FileInfo> files)
        {

        }

        public async Task<FileProperties> IdentifyFileAsync(Stream stream)
        {
            try
            {
                Exporter exporter = _exporterFactory.CreateExporter();

                stream.Position = 0;
                long fileSize = await GetFileSizeAsync(stream).ConfigureAwait(false);

                stream.Position = 0;
                FileFormat fileFormat = _outsideInService.IdentifyFile(stream, exporter);

                return new FileProperties(fileFormat.GetId(), fileFormat.GetDescription(), fileSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file properties");
                throw;
            }
        }

        private async Task<long> GetFileSizeAsync(Stream stream)
        {
            try
            {
                return stream.Length;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get file size");
                throw;
            }
        }
    }
}
