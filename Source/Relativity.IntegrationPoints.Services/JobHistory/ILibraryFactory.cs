using kCura.IntegrationPoints.Data;

namespace Relativity.IntegrationPoints.Services.JobHistory
{
	public interface ILibraryFactory
	{
		IGenericLibrary<T> Create<T>(int workspaceId) where T : BaseRdo, new();
	}
}