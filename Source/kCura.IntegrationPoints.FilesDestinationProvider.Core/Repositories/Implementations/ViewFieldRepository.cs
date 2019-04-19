using kCura.IntegrationPoints.Common.Constants;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using Relativity.Services.Interfaces.ViewField;
using Relativity.Services.Interfaces.ViewField.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations
{
	public class ViewFieldRepository : IViewFieldRepository
	{
		private readonly IViewFieldManager _viewFieldManager;
		private readonly IExternalServiceInstrumentationProvider _instrumentationProvider;

		public ViewFieldRepository(
			IViewFieldManager viewFieldManager,
			IExternalServiceInstrumentationProvider instrumentationProvider)
		{
			_viewFieldManager = viewFieldManager;
			_instrumentationProvider = instrumentationProvider;
		}

		public ViewFieldResponse[] ReadExportableViewFields(int workspaceID, int artifactTypeID)
		{
			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(IViewFieldManager.ReadExportableViewFieldsAsync)
			);
			return instrumentation.ExecuteAsync(
				() => _viewFieldManager.ReadExportableViewFieldsAsync(workspaceID, artifactTypeID)
			)
			.GetAwaiter()
			.GetResult();
		}

		public ViewFieldIDResponse[] ReadViewFieldIDsFromSearch(int workspaceID, int artifactTypeID, int viewArtifactID)
		{
			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(IViewFieldManager.ReadViewFieldIDsFromSearchAsync)
			);
			return instrumentation.ExecuteAsync(
				() => _viewFieldManager.ReadViewFieldIDsFromSearchAsync(workspaceID, artifactTypeID, viewArtifactID)
			)
			.GetAwaiter()
			.GetResult();
		}

		public ViewFieldIDResponse[] ReadViewFieldIDsFromProduction(int workspaceID, int artifactTypeID, int viewArtifactID)
		{
			IExternalServiceSimpleInstrumentation instrumentation = CreateInstrumentation(
				operationName: nameof(IViewFieldManager.ReadViewFieldIDsFromProductionAsync)
			);
			return instrumentation.ExecuteAsync(
				() =>_viewFieldManager.ReadViewFieldIDsFromProductionAsync(workspaceID, artifactTypeID, viewArtifactID)
			)
			.GetAwaiter()
			.GetResult();
		}

		private IExternalServiceSimpleInstrumentation CreateInstrumentation(string operationName)
		{
			return _instrumentationProvider.CreateSimple(
				ExternalServiceTypes.KEPLER,
				nameof(IViewFieldManager),
				operationName);
		}
	}
}
