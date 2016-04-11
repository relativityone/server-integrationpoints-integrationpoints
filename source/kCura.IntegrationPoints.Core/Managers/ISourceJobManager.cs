using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	/// <summary>
	/// Responsible for handling all work for the Source Job rdo
	/// </summary>
	public interface ISourceJobManager
	{
		/// <summary>
		/// Initializes the given workspace with the Source Job rdo, fields, and a new instance
		/// </summary>
		/// <param name="sourceWorkspaceArtifactId">The artifact id of the source workspace</param>
		/// <param name="destinationWorkspaceArtifactId">The artifact id of the destination workspace</param>
		/// <param name="sourceWorkspaceArtifactTypeId">The artifact type Id of the Source Workspace rdo</param>
		/// <param name="sourceWorkspaceRDOInstanceArtifactId">The artifact id of the instance of the parent Source Workspace rdo to associate the new Source Job with</param>
		/// <param name="jobHistoryArtifactId">The artifact id of the Job History rdo from the Source Workspace</param>
		/// <returns>A SourceJobDTO object for the newly created instance of Source Job</returns>
		SourceJobDTO InitializeWorkspace(
			int sourceWorkspaceArtifactId,
			int destinationWorkspaceArtifactId,
			int sourceWorkspaceArtifactTypeId,
			int sourceWorkspaceRDOInstanceArtifactId,
			int jobHistoryArtifactId);
	}
}