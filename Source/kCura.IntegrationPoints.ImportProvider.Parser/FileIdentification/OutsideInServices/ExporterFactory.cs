using OutsideIn;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification.OutsideInServices
{
    public class ExporterFactory : IExporterFactory
    {
        public Exporter CreateExporter()
        {
            return global::OutsideIn.OutsideIn.NewLocalExporter();
        }
    }
}
