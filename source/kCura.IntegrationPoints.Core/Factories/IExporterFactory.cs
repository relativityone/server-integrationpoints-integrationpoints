using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IExporterFactory
	{
		IExporterService BuildExporter(FieldMap[] mappedFiles, string config, int savedSearchArtifactId, int onBehalfOfUser);
	}
}