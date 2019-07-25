using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
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
			int onBehalfOfUser, 
			string userImportApiSettings,
			IDocumentRepository documentRepository,
			ISerializer serializer);
	}
}