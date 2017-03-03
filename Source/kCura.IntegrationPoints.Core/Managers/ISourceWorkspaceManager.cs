using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface ISourceWorkspaceManager
	{
		/// <summary>
		///     Creates the necessary objects and fields on the destination workspace to indicate the
		///     source of a document.
		/// </summary>
		/// <param name="sourceWorkspaceArtifactId">The source workspace artifact ID.</param>
		/// <param name="destinationWorkspaceArtifactId">The destination workspace artifact ID.</param>
		/// <param name="federatedInstanceArtifactId">The federated instace artifact ID.</param>
		/// <returns>The source workspace DTO.</returns>
		SourceWorkspaceDTO InitializeWorkspace(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, int? federatedInstanceArtifactId);
	}
}