using OutsideIn;

namespace kCura.IntegrationPoints.ImportProvider.FileIdentification.OutsideInServices
{
    public class ExporterFactory : IExporterFactory
    {
        public Exporter CreateExporter()
        {
            return global::OutsideIn.OutsideIn.NewLocalExporter();
        }
    }
}