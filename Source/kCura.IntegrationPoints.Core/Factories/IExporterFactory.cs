using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Factories
{
    public interface IExporterFactory
    {
        IExporterService BuildExporter(
            IJobStopManager jobStopManager,
            FieldMap[] mappedFields,
            string config,
            int savedSearchArtifactID,
            ImportSettings userImportApiSettings,
            IDocumentRepository documentRepository,
            IExportDataSanitizer exportDataSanitizer);
    }
}
