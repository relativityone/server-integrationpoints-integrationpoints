using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ExporterFactory : IExporterFactory
	{
		private readonly IFieldManager _fieldManager;
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;

		public ExporterFactory(IFieldManager fieldManager, ISourceWorkspaceManager sourceWorkspaceManager)
		{
			_fieldManager = fieldManager;
			_sourceWorkspaceManager = sourceWorkspaceManager;
		}

		public IExporterService BuildExporter(FieldMap[] mappedFiles, string config)
		{
			return new RelativityExporterService(_fieldManager, _sourceWorkspaceManager, mappedFiles, 0, config);
		}
	}
}