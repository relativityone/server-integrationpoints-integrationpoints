using System;
using System.IO;
using kCura.IntegrationPoints.Common.Handlers;
using OutsideIn;
using Relativity.API;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification.OutsideInServices
{
    public class OutsideInService : IOutsideInService
    {
        private readonly IExporterFactory _exporterFactory;
        private readonly IRetryHandler _retryHandler;
        private readonly IAPILog _logger;

        public OutsideInService(IExporterFactory exporterFactory, IRetryHandler retryHandler, IAPILog logger)
        {
            _exporterFactory = exporterFactory;
            _retryHandler = retryHandler;
            _logger = logger;
        }

        public FileFormat IdentifyFile(Stream stream)
        {
            using (Exporter exporter = _exporterFactory.CreateExporter())
            {
                try
                {
                    // as we do not have precise retry-policy configuration in RIP, we retry on every exception type here
                    return _retryHandler.Execute<FileFormat, Exception>(
                        () =>
                        {
                            stream.Position = 0;
                            exporter.SetSourceFile(stream);
                            return exporter.GetFileId(FileIdInfoFlagValue.Normal);
                        },
                        exception =>
                        {
                            _logger.LogWarning(exception, "Unable to identify file using OutsideIn.Exporter. Operation will be retried.");
                        });
                }
                catch (Exception ex)
                {
                    // if retries did not helped, our last hope is reading file metadata from metrics
                    return GetFileFormatFromMetrics(exporter) ?? throw ex;
                }
            }
        }

        private FileFormat GetFileFormatFromMetrics(Exporter exporter)
        {
            var metrics = exporter.GetExportMetrics();
            if (metrics.TryGetValue("oi.file_id", out object fileId))
            {
                var id = Convert.ToInt32(fileId);
                if (id != 0 && id != FileFormat.UNMAPPEDSCCFI.GetId())
                {
                    return FileFormat.ForId(id);
                }
            }

            return null;
        }
    }
}
