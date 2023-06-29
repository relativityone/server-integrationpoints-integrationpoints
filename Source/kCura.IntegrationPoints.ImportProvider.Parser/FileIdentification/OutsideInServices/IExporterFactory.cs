using OutsideIn;

namespace kCura.IntegrationPoints.ImportProvider.Parser.FileIdentification.OutsideInServices
{
    public interface IExporterFactory
    {
        Exporter CreateExporter();
    }
}
