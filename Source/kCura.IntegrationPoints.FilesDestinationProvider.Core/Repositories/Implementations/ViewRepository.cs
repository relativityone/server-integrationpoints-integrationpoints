using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.Services.View;
using Relativity.Services.ViewManager.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations
{
	public class ViewRepository : IViewRepository
	{
		private readonly IViewManager _viewManager;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

		public ViewRepository(
			IViewManager viewManager, 
			IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_viewManager = viewManager;
			_instrumentationProvider = instrumentationProvider;
		}

		public ViewResponse[] RetrieveViewsByContextArtifactID(int workspaceArtifactID, int artifactTypeID)
		{
			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(IViewManager.RetrieveViewsByContextArtifactIDAsync)
			);
			return instrumentation.ExecuteAsync(
					() => _viewManager.RetrieveViewsByContextArtifactIDAsync(workspaceArtifactID, artifactTypeID)
				)
				.GetAwaiter()
				.GetResult();
		}

		public SearchViewResponse[] RetrieveViewsByContextArtifactIDForSearch(int workspaceArtifactID)
		{
			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(IViewManager.RetrieveViewsByContextArtifactIDForSearchAsync)
			);
			return instrumentation.ExecuteAsync(
					() => _viewManager.RetrieveViewsByContextArtifactIDForSearchAsync(workspaceArtifactID)
				)
				.GetAwaiter()
				.GetResult();
		}

		private IExternalServiceSimpleInstrumentation CreateInstrumentation(string operationName)
		{
			return _instrumentationProvider.CreateSimple(
				ExternalServiceTypes.KEPLER,
				nameof(IViewManager),
				operationName);
		}
	}
}
