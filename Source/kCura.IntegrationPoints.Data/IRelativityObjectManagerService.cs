using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data
{
	public interface IRelativityObjectManagerService
	{
		// all references to IGenericLibrary except mass operations should be removed
		IGenericLibrary<JobHistoryError> JobHistoryErrorLibrary { get; }
		IRelativityObjectManager RelativityObjectManager { get; }
	}
}
