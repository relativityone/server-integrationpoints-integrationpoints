using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class ExporterFactory
	{
		public static IExporterService BuildExporter(FieldMap[] mappedFiles, string config, DirectSqlCallHelper helper)
		{
			return new RelativityExporterService(mappedFiles, 0, config, helper);
		}
	}
}