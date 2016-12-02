using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface ILibraryFactory
	{
		IGenericLibrary<T> Create<T>(int workspaceId) where T : BaseRdo, new();
	}
}