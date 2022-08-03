using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IExportInitProcessService
    {
        long CalculateDocumentCountToTransfer(ExportUsingSavedSearchSettings exportSettings, int artifactTypeId);
    }
}
