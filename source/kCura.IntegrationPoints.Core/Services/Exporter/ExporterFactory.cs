using Castle.Windsor;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Managers;
using kCura.IntegrationPoints.Data.Managers.Implementations;
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class ExporterFactory
	{
		public static IExporterService BuildExporter(DirectSqlCallHelper queryHelper, FieldMap[] mappedFiles, string config)
		{
			return new RelativityExporterService(queryHelper, mappedFiles, 0, config);
		}
	}
}