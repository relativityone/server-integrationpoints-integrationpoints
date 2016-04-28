using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Responsible for handling Destination Workspace RDOs and their functionality.
	/// </summary>
	public interface IDestinationWorkspaceRepository
	{
		/// <summary>
		/// Queries to see if a Destination Workspace RDO instance exists for the corresponding target workspace.
		/// </summary>
		/// <param name="destinationWorkspaceId">The Artifact ID of the workspace we are looking for (note, this is NOT
		/// the Artifact ID of the object instance, it's the Artifact ID of the actual workspace</param>
		/// <returns>null if no instance exists, DestinationWorkspaceDTO of instance otherwise</returns>
		DestinationWorkspaceDTO QueryDestinationWorkspaceRdoInstance(int destinationWorkspaceId);

		/// <summary>
		/// Creates an instance of a Destination Workspace RDO.
		/// </summary>
		/// <param name="destinationWorkspaceId">Artifact ID of the DestinationWorkspace</param>
		/// <param name="destinationWorkspaceName">Name of the DestinationWorkspace RDO instance</param>
		/// <returns>DestinationWorkspaceDTO of the instance that was just created</returns>
		DestinationWorkspaceDTO CreateDestinationWorkspaceRdoInstance(int destinationWorkspaceId, string destinationWorkspaceName);

		/// <summary>
		/// Mass edits/tags the Documents that were with the corresponding Destination Workspace they were pushed to.
		/// </summary>
		/// <param name="numberOfDocs">The number of documents to tag</param>
		/// <param name="destinationWorkspaceInstanceId">Artifact ID of the DestinationWorkspace RDO instance</param>
		/// <param name="tableSuffix">Unique suffix that is appended to the scratch table</param>
		/// <param name="sourceWorkspaceId">Artifact ID of the source workspace</param>
		void TagDocsWithDestinationWorkspace(int numberOfDocs, int? destinationWorkspaceInstanceId, string tableSuffix,
			int sourceWorkspaceId);

		/// <summary>
		/// Links the multi-object fields on DestinationWorkspace and JobHistory objects to each other.
		/// </summary>
		/// <param name="destinationWorkspaceInstanceId">Artifact ID of the DestinationWorkspace RDO instance</param>
		/// <param name="jobHistoryInstanceId">Artifact ID of the JobHistory RDO instance</param>
		void LinkDestinationWorkspaceToJobHistory(int? destinationWorkspaceInstanceId, int jobHistoryInstanceId);

		/// <summary>
		/// Update the Destination Workspace RDO.
		/// </summary>
		/// <param name="destinationWorkspace">The DTO of the Destination Workspace to update</param>
		void UpdateDestinationWorkspaceRdoInstance(DestinationWorkspaceDTO destinationWorkspace);
	}
}
