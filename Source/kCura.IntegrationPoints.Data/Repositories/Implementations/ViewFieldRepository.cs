using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.Services.Interfaces.ViewField;
using Relativity.Services.Interfaces.ViewField.Models;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class ViewFieldRepository : IViewFieldRepository
	{
		private readonly IViewFieldManager _viewFieldManager;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;
		private readonly int _workspaceID;

		public ViewFieldRepository(IViewFieldManager viewFieldManager,
			IExternalServiceInstrumentationProvider instrumentationProvider, 
			int workspaceID)
		{
			_viewFieldManager = viewFieldManager;
			_instrumentationProvider = instrumentationProvider;
			_workspaceID = workspaceID;
		}

		public ViewFieldResponse[] GetAllViewFieldsByArtifactTypeID(int artifactTypeID)
		{
			IExternalServiceSimpleInstrumentation instrumentation = _instrumentationProvider.CreateSimple(
				ExternalServiceTypes.KEPLER,
				nameof(IViewFieldManager),
				nameof(IViewFieldManager.GetAllViewFieldsByArtifactTypeIDAsync));

			return instrumentation.Execute(() =>
				_viewFieldManager.GetAllViewFieldsByArtifactTypeIDAsync(_workspaceID, artifactTypeID).GetAwaiter().GetResult());
		}

		public ViewFieldIDResponse[] GetViewFieldsByArtifactTypeIDAndViewArtifactID(int artifactTypeID, 
			int viewArtifactID,
			bool fromProduction)
		{
			IExternalServiceSimpleInstrumentation instrumentation = _instrumentationProvider.CreateSimple(
				ExternalServiceTypes.KEPLER,
				nameof(IViewFieldManager),
				nameof(IViewFieldManager.GetViewFieldsByArtifactTypeIDAndViewArtifactIDAsync));

			return instrumentation.Execute(() =>
				_viewFieldManager
					.GetViewFieldsByArtifactTypeIDAndViewArtifactIDAsync(_workspaceID, artifactTypeID, viewArtifactID, fromProduction)
					.GetAwaiter().GetResult());
		}
	}
}
