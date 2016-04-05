using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services.Exporter;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IExporterFactory
	{
		IExporterService BuildExporter(FieldMap[] mappedFiles, string config);
	}
}