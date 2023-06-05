using System;
using System.IO;
using OutsideIn;
using Relativity.API;

namespace kCura.IntegrationPoints.ImportProvider.FileIdentification.OutsideInServices
{
    public class OutsideInService : IOutsideInService
    {
        private readonly IAPILog _logger;

        public OutsideInService(IAPILog logger)
        {
            _logger = logger;
        }

        public FileFormat IdentifyFile(Stream stream, Exporter exporter)
        {
            try
            {
                exporter.SetSourceFile(stream);
                FileFormat fileFormat = exporter.GetFileId(FileIdInfoFlagValue.Normal);
                return fileFormat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to identify file");
                throw;
            }
        }
    }
}