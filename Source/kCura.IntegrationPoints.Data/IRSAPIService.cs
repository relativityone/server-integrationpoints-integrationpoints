using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data
{
	public partial interface IRSAPIService
	{
		// all references to IGenericLibrary except mass operations should be removed
		IGenericLibrary<IntegrationPoint> IntegrationPointLibrary { get; }
		IGenericLibrary<SourceProvider> SourceProviderLibrary { get; }
		IGenericLibrary<JobHistoryError> JobHistoryErrorLibrary { get; }
		IGenericLibrary<DestinationWorkspace> DestinationWorkspaceLibrary { get; }
		
		IRelativityObjectManager RelativityObjectManager { get; }

		IGenericLibrary<T> GetGenericLibrary<T>() where T : BaseRdo, new();
	}
}
