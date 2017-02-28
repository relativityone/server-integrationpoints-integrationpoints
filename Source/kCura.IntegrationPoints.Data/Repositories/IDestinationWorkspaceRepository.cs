using System.Security.Claims;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	///     Responsible for handling Destination Workspace RDOs and their functionality.
	/// </summary>
	public interface IDestinationWorkspaceRepository
	{
		/// <summary>
		///     Queries to see if a Destination Workspace RDO instance exists for the corresponding target workspace.
		/// </summary>
		/// <param name="targetWorkspaceArtifactId">
		///     The Artifact ID of the workspace we are looking for (note, this is NOT
		///     the Artifact ID of the object instance, it's the Artifact ID of the actual workspace
		/// </param>
		/// <param name="federatedInstanceArtifactId">The ArtifactId of the Federated Instance</param>
		/// <returns>null if no instance exists, DestinationWorkspaceDTO of instance otherwise</returns>
		DestinationWorkspace Query(int targetWorkspaceArtifactId, int? federatedInstanceArtifactId);

		/// <summary>
		///     Creates an instance of a Destination Workspace RDO.
		/// </summary>
		/// <param name="targetWorkspaceArtifactId">Artifact ID of the target workspace</param>
		/// <param name="targetWorkspaceName">Name of the target workspace</param>
		/// <param name="federatedInstanceArtifactId">The ArtifactId of the Federated Instance</param>
		/// <param name="federatedInstanceName">Name of the Federated Instance</param>
		/// <returns>DestinationWorkspaceDTO of the instance that was just created</returns>
		DestinationWorkspace Create(int targetWorkspaceArtifactId, string targetWorkspaceName, int? federatedInstanceArtifactId, string federatedInstanceName);

		/// <summary>
		///     Mass edits/tags the Documents that were with the corresponding Destination Workspace they were pushed to.
		/// </summary>
		/// <param name="claimsPrincipal">A ClaimsPrincipal object that contains the identity of the user</param>
		/// <param name="numberOfDocs">The number of documents to tag</param>
		/// <param name="destinationWorkspaceInstanceId">Artifact ID of the DestinationWorkspace RDO instance</param>
		/// <param name="jobHistoryInstanceId">Artifact ID of the JobHistory RDO instance</param>
		/// <param name="tableSuffix">Unique suffix that is appended to the scratch table</param>
		/// <param name="sourceWorkspaceId">Artifact ID of the source workspace</param>
		void TagDocsWithDestinationWorkspaceAndJobHistory(ClaimsPrincipal claimsPrincipal, int numberOfDocs, int destinationWorkspaceInstanceId, int jobHistoryInstanceId,
			string tableSuffix,
			int sourceWorkspaceId);

		/// <summary>
		///     Links the multi-object fields on DestinationWorkspace and JobHistory objects to each other.
		/// </summary>
		/// <param name="destinationWorkspaceInstanceId">Artifact ID of the DestinationWorkspace RDO instance</param>
		/// <param name="jobHistoryInstanceId">Artifact ID of the JobHistory RDO instance</param>
		void LinkDestinationWorkspaceToJobHistory(int destinationWorkspaceInstanceId, int jobHistoryInstanceId);

		/// <summary>
		///     Update the Destination Workspace RDO.
		/// </summary>
		/// <param name="destinationWorkspace">The DTO of the Destination Workspace to update</param>
		void Update(DestinationWorkspace destinationWorkspace);
	}
}