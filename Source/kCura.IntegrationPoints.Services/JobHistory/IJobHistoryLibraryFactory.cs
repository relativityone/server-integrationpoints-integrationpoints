using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface IJobHistoryLibraryFactory
	{
		IGenericLibrary<Data.JobHistory> Create(int workspaceId);
	}
}