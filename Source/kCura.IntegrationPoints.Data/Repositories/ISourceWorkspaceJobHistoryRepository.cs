using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Responsible for handling the Job History object from a source workspace 
	/// </summary>
	public interface ISourceWorkspaceJobHistoryRepository
	{
		/// <summary>
		/// Retrieves the Job History with the given artifact id
		/// </summary>
		/// <param name="jobHistoryArtifactId">The artifact of the Job History rdo</param>
		/// <returns>A SourceWorkspaceJobHistoryDTO object representing the Job History rdo</returns>
		SourceWorkspaceJobHistoryDTO Retrieve(int jobHistoryArtifactId);
	}
}