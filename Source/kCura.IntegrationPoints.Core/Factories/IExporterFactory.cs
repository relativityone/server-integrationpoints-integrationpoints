using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IExporterFactory
	{
		IExporterService BuildExporter(
			IJobStopManager jobStopManager, 
			FieldMap[] mappedFields, 
			string config, 
			int savedSearchArtifactID,
			string userImportApiSettings,
			IDocumentRepository documentRepository,
			IExportDataSanitizer exportDataSanitizer);
	}
}