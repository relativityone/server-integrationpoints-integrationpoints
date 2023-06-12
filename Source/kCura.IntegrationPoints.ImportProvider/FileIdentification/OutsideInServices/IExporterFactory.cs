using OutsideIn;

namespace kCura.IntegrationPoints.ImportProvider.FileIdentification.OutsideInServices
{
    public interface IExporterFactory
    {
        Exporter CreateExporter();
    }
}