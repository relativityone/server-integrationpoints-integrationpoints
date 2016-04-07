using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ExporterFactory : IExporterFactory
	{
		private readonly ISourceWorkspaceManager _sourceWorkspaceManager;
		private readonly ITargetWorkspaceJobHistoryManager _targetWorkspaceJobHistoryManager;

		public ExporterFactory(ISourceWorkspaceManager sourceWorkspaceManager, ITargetWorkspaceJobHistoryManager targetWorkspaceJobHistoryManager)
		{
			_sourceWorkspaceManager = sourceWorkspaceManager;
			_targetWorkspaceJobHistoryManager = targetWorkspaceJobHistoryManager;
		}

		public IExporterService BuildExporter(FieldMap[] mappedFiles, string config)
		{
			return new RelativityExporterService(_sourceWorkspaceManager, _targetWorkspaceJobHistoryManager, mappedFiles, 0, config);
		}
	}
}