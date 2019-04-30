using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
	internal interface IJobHistoryErrorRepository
	{
		Task<IJobHistoryError> CreateAsync(int workspaceArtifactId, CreateJobHistoryErrorDto createJobHistoryErrorDto);
	}
}