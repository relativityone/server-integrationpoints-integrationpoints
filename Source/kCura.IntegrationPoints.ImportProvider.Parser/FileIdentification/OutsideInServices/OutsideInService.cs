using System;
using System.IO;
using OutsideIn;
using Relativity.API;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification.OutsideInServices
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
            // TODO Retry
            // OutsideIn.OutsideInException: OI process was killed by the OS due to a fatal exception
            // at OutsideIn.DocumentImpl.GetFileId(FileIdInfoFlagValue dwFlags)
            // at FIleIdentify.FileIdentification.FileIdentificationStage.<>c__DisplayClass13_0.<StartConsumer>b__0()

            // OILink.OILinkErrorCodeException: OI DAOpenDocument failed - 13: not enough memory for allocation [13]
            // at OutsideIn.DocumentImpl.GetFileId(FileIdInfoFlagValue dwFlags)
            // at FIleIdentify.FileIdentification.FileIdentificationStage.<>c__DisplayClass13_0.<StartConsumer>b__0()

            // OILink.OILinkErrorCodeException: OI DAOpenDocument failed - 65535: OI error code could not be mapped [7172963]
            // at OutsideIn.DocumentImpl.GetFileId(FileIdInfoFlagValue dwFlags)
            // at FIleIdentify.FileIdentification.FileIdentificationStage.<>c__DisplayClass13_0.<StartConsumer>b__0()


            // FileTypeIdentificationException >> metrics


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
