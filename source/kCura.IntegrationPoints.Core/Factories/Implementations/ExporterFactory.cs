using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services.Exporter;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ExporterFactory : IExporterFactory
	{
		public IExporterService BuildExporter(FieldMap[] mappedFiles, string config)
		{
			return new RelativityExporterService(mappedFiles, 0, config);
		}
	}
}